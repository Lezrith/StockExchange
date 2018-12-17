using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Data;
using Model;

namespace Logic.Matcher
{
    public class Matcher
    {
        private readonly IList<string> companies;
        private readonly CassandraContext context;
        private readonly Random rng;
        private readonly TimeSpan period;
        private readonly Guid matcherId;

        public Matcher(ICollection<string> companies, CassandraContext context, Random rng, TimeSpan period)
        {
            this.companies = companies.ToList() ?? throw new ArgumentNullException(nameof(companies));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
            this.period = period;
            this.matcherId = Guid.NewGuid();
        }

        public void Run()
        {
            while (true)
            {
                var company = this.companies[this.rng.Next(this.companies.Count)];
                this.CreateTransaction(company);
                Thread.Sleep(this.period);
            }
        }

        private void CreateTransaction(string company)
        {
            var orders = this.context.FetchOrders(company).Where(o => !o.LockedBy.Any()).ToList();
            var purchases = orders.Where(o => o.OrderType == OrderType.Purchase);
            var sales = orders.Where(o => o.OrderType == OrderType.Sale);
            var (sale, purchase) = sales
                .Join(purchases, s => s, p => p, (s, p) => (s, p), new OrderMatchComparer())
                .FirstOrDefault();
            if (sale != null && purchase != null)
            {
                var toLock = new List<Order> { sale, purchase };
                this.context.LockOrders(toLock, this.matcherId);
                if (this.context.HasExclusiveLock(toLock, this.matcherId))
                {
                    this.context.MakeTransaction(purchase, sale, this.matcherId);
                }
                else
                {
                    this.context.UnlockOrders(toLock, this.matcherId);
                }
            }
        }

        private class OrderMatchComparer : IEqualityComparer<Order>
        {
            public bool Equals(Order x, Order y)
            {
                if (x.OrderType == y.OrderType)
                {
                    return false;
                }
                var purchase = x.OrderType == OrderType.Purchase ? x : y;
                var sale = x.OrderType == OrderType.Sale ? x : y;

                return sale.Quantity >= purchase.Quantity && sale.PricePerUnit <= purchase.PricePerUnit;
            }

            public int GetHashCode(Order obj)
            {
                return 0;
            }
        }
    }
}

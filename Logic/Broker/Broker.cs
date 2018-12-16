using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Data;
using Model;

namespace Logic.Broker
{
    public class Broker
    {
        private readonly IList<string> companies;
        private readonly CassandraContext context;
        private readonly Random random;
        private readonly TimeSpan period;

        private readonly Guid myId;

        public string Name { get; }

        public Broker(ICollection<string> companies, CassandraContext context, string name, TimeSpan period)
        {
            this.companies = companies.ToList();
            this.context = context;

            this.random = new Random();

            this.myId = Guid.NewGuid();
            this.Name = name;

            this.period = period;
        }

        public void Run()
        {
            while(true)
            {
                this.CreateOrder();
                Thread.Sleep(this.period);
            }
        }

        private void CreateOrder()
        {
            var company = this.companies[this.random.Next(this.companies.Count)];
            var orderType = (OrderType)this.random.Next(2);

            var meanPrice = this.context.GetAverageTransactionPrice(company, DateTimeOffset.Now - TimeSpan.FromMinutes(2));
            if (meanPrice == 0) // no entries
            {
                meanPrice = 50;
            }

            var myPrice =  Math.Max(meanPrice + (decimal)(this.random.NextDouble() * 20f - 10f), 1m);
            var myQuantity = this.random.Next(10, 100);

            var newOrder = new Order
            {
                OrderId = Guid.NewGuid(),
                StockSymbol = company,
                SubmitterId = myId,
                SubmitterName = Name,
                Quantity = myQuantity,
                OrderType = orderType,
                PricePerUnit = (decimal)myPrice,
                Date = DateTimeOffset.Now,
                LockedBy = null,
            };

            this.context.InsertOrder(newOrder);
        }
    }
}

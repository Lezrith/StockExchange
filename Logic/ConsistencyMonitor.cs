using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Data;
using Microsoft.Extensions.Logging;

namespace Logic
{
    public class ConsistencyMonitor
    {
        private readonly CassandraContext context;
        private readonly ILogger logger;
        private readonly IEnumerable<string> companies;
        private readonly TimeSpan period;

        public ConsistencyMonitor(
            IEnumerable<string> companies,
            CassandraContext context,
            ILogger logger,
            TimeSpan period)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.companies = companies ?? throw new ArgumentNullException(nameof(companies));
            this.period = period;
        }

        public void Start()
        {
            DateTimeOffset lastRun = DateTimeOffset.MinValue;
            while (true)
            {
                using (this.logger.BeginScope("Monitor scope"))
                {
                    foreach (var company in this.companies)
                    {
                        this.logger.LogInformation($"Checking consistency for {company} after {lastRun}");
                        var transactions = this.context.FetchTransactions(company, lastRun).ToList();
                        var uniquePurchaseOrders = transactions.GroupBy(t => t.PurchaseOrderId).ToList();
                        var uniqueSaleOrders = transactions.GroupBy(t => t.SaleOrderId).Distinct().ToList();
                        this.logger.LogInformation($"Found {transactions.Count} transactions created from {uniqueSaleOrders.Count} " +
                            $"unique saleorders and {uniquePurchaseOrders.Count} unique purchase orders.");

                        var notUnique = uniquePurchaseOrders.Where(p => p.Count() > 1).Union(uniqueSaleOrders.Where(p => p.Count() > 1))
                            .SelectMany(t => t.Select(t2 => t2.TransactionId))
                            .ToList();
                        var message = String.Join(", ", notUnique.Select(p => p.ToString()));
                        if (notUnique.Count > 0)
                        {
                            this.logger.LogInformation($"Potentially erroneous transactions are: {message}");
                        }
                    }
                }
                lastRun = DateTime.UtcNow;
                Thread.Sleep(this.period);
            }
        }
    }
}

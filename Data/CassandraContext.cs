using System;
using System.Collections.Generic;
using System.Linq;
using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.Logging;
using Model;

namespace Data
{
    public class CassandraContext
    {
        private readonly ISession session;
        private readonly Mapper mapper;

        public static void AddLogger(ILoggerProvider logger)
        {
            Diagnostics.AddLoggerProvider(logger);
        }

        public CassandraContext() : this(new CassandraOptions())
        {
        }

        public CassandraContext(CassandraOptions options)
        {
            var cluster = Cluster.Builder()
                .AddContactPoints(options.ContactPoints)
                .Build();
            this.session = cluster.Connect();
            this.mapper = new Mapper(this.session);
            this.ConfigureCluster();
            this.session.ChangeKeyspace(options.Keyspace);

            MappingConfiguration.Global.Define<CassandraMappings>();
        }

        public string GetCqlVersion()
        {
            return this.mapper.Single<string>("SELECT cql_version FROM system.local");
        }

        private void ConfigureCluster()
        {
            var keyspaceCreate = this.session.Execute("CREATE KEYSPACE IF NOT EXISTS stock_exchange " +
                "WITH REPLICATION = { 'class': 'SimpleStrategy', 'replication_factor': 3 };");
            var keyspaceUse = this.session.Execute("USE stock_exchange");
            var orderCreate = this.session.Execute("CREATE TABLE IF NOT EXISTS stock_exchange.orders (" +
                "OrderId uuid," +
                "StockSymbol text," +
                "SubmitterId uuid," +
                "SubmitterName text," +
                "Quantity int," +
                "OrderType int," +
                "PricePerUnit decimal," +
                "Date timestamp," +
                "LockedBy set<uuid>," +
                "PRIMARY KEY (StockSymbol, OrderId));");
            var transactionCreate = this.session.Execute("CREATE TABLE IF NOT EXISTS stock_exchange.transactions (" +
                "TransactionId uuid," +
                "StockSymbol text," +
                "BuyerId uuid," +
                "BuyerName text," +
                "SellerId uuid," +
                "SellerName text," +
                "Quantity int," +
                "PricePerUnit decimal," +
                "Date timestamp," +
                "PRIMARY KEY (StockSymbol, Date));");
        }

        public void InsertOrder(Order order)
        {
            this.mapper.Insert(order, CqlQueryOptions.New().SetConsistencyLevel(ConsistencyLevel.Any));
        }

        public void RemoveOrder(Order order)
        {
            this.mapper.Delete(order);
        }

        public void InsertTransaction(Transaction transaction)
        {
            this.mapper.Insert(transaction, CqlQueryOptions.New().SetConsistencyLevel(ConsistencyLevel.Any));
        }

        public IEnumerable<Order> FetchOrders(string stockSymbol)
        {
            var query = Cql.New("SELECT * FROM orders WHERE StockSymbol = ?", stockSymbol);
            query.WithOptions(o => o.SetConsistencyLevel(ConsistencyLevel.One));
            return this.mapper.Fetch<Order>(query);
        }

        public void LockOrders(IEnumerable<Order> orders, Guid matcherId)
        {
            foreach (var order in orders)
            {
                var statement = new SimpleStatement(
                    $"UPDATE orders SET LockedBy = LockedBy + {{{matcherId}}} WHERE StockSymbol = ? AND OrderId = ?",
                    order.StockSymbol,
                    order.OrderId);
                statement.SetConsistencyLevel(ConsistencyLevel.Quorum);
                this.session.Execute(statement);
            }
        }

        public void UnlockOrders(IEnumerable<Order> orders, Guid matcherId)
        {
            foreach (var order in orders)
            {
                var statement = new SimpleStatement(
                    $"UPDATE orders SET LockedBy = LockedBy - {{{matcherId}}} WHERE StockSymbol = ? AND OrderId = ?",
                    order.StockSymbol,
                    order.OrderId);
                statement.SetConsistencyLevel(ConsistencyLevel.Quorum);
                this.session.Execute(statement);
            }
        }

        public bool HaveLock(IEnumerable<Order> orders, Guid matcherId)
        {
            return orders.All(order =>
            {
                var query = Cql.New(
                    "SELECT LockedBy FROM orders WHERE StockSymbol = ? AND OrderId = ?",
                    order.StockSymbol,
                    order.OrderId);
                query.WithOptions(o => o.SetConsistencyLevel(ConsistencyLevel.Quorum));
                return this.mapper.Single<IEnumerable<Guid>>(query).Contains(matcherId);
            });
        }

        public void MakeTransaction(Order purchase, Order sale)
        {
            var batch = this.mapper.CreateBatch();
            batch.Delete(purchase);
            batch.Delete(sale);
            var difference = sale.Quantity - purchase.Quantity;
            if (difference < 0)
            {
                purchase.OrderId = new Guid();
                purchase.Quantity = -difference;
                batch.Insert(purchase);
            }
            else if (difference > 0)
            {
                sale.OrderId = new Guid();
                sale.Quantity = difference;
                batch.Insert(sale);
            }
            var transaction = Transaction.FromOrders(purchase, sale);
            batch.Insert(transaction);
            this.mapper.Execute(batch);
        }

        public IEnumerable<Transaction> FetchTransactions(string stockSymbol, DateTimeOffset minDate)
        {
            var query = Cql.New("SELECT * FROM transactions WHERE stockSymbol = ? AND date > ?", stockSymbol, minDate);
            query.WithOptions(o => o.SetConsistencyLevel(ConsistencyLevel.One));
            return this.mapper.Fetch<Transaction>(query);
        }

        public decimal GetAverageTransactionPrice(string stockSymbol, DateTimeOffset minDate)
        {
            var query = Cql.New("SELECT avg(pricePerUnit) FROM transactions WHERE stockSymbol = ? AND date > ?", stockSymbol, minDate);
            query.WithOptions(o => o.SetConsistencyLevel(ConsistencyLevel.One));
            return this.mapper.Single<decimal>(query);
        }
    }
}

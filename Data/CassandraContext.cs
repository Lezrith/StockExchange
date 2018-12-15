using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.Logging;

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
            this.session = cluster.Connect(options.Keyspace);
            this.mapper = new Mapper(this.session);
            this.ConfigureCluster();
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
                "PRIMARY KEY ((StockSymbol, OrderType)));");
            var transactionCreate = this.session.Execute("CREATE TABLE IF NOT EXISTS stock_exchange.transactions (" +
                "TransactionId uuid," +
                "StockSymbol text," +
                "BuyerId uuid," +
                "BuyerName text," +
                "SelledId uuid," +
                "SellerName text," +
                "Quantity int," +
                "PricePerUnit decimal," +
                "Date timestamp," +
                "PRIMARY KEY (StockSymbol, Date));");
        }
    }
}

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
        }

        public string GetCqlVersion()
        {
            return this.mapper.Single<string>("SELECT cql_version FROM system.local");
        }
    }
}

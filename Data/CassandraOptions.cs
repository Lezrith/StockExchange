namespace Data
{
    public class CassandraOptions
    {
        public CassandraOptions()
        {
            this.ContactPoints = new[] { "127.0.0.1" };
            this.Keyspace = "stock_exchange";
            this.ReplicationStrategy = "SimpleStrategy";
            this.ReplicationFactor = 3;
        }

        public string[] ContactPoints { get; set; }

        public string Keyspace { get; set; }

        public string ReplicationStrategy { get; set; }

        public int ReplicationFactor { get; set; }
    }
}

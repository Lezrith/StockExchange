namespace Data
{
    public class CassandraOptions
    {
        public CassandraOptions()
        { }

        public string[] ContactPoints { get; set; }

        public string Keyspace { get; set; }

        public string ReplicationStrategy { get; set; }

        public int ReplicationFactor { get; set; }
    }
}

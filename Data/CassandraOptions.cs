namespace Data
{
    public class CassandraOptions
    {
        public CassandraOptions()
        {
            this.ContactPoints = new[] { "127.0.0.1" };
            this.Keyspace = "";
        }

        public string[] ContactPoints { get; set; }

        public string Keyspace { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data;

namespace Logic.Broker
{
    /// <summary>
    /// Starts its own thread and creates orders forever.
    /// </summary>
    public class BrokerManager : IManager
    {
        private Task[] tasks;
        private readonly IList<string> companies;
        private readonly CassandraContext context;
        private readonly string name;
        private TimeSpan period;

        public BrokerManager(IList<string> companies, CassandraContext context, string name, TimeSpan period)
        {
            this.companies = companies ?? throw new ArgumentNullException(nameof(companies));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.period = period;
        }

        public void Start(int numberOfThreads)
        {
            this.tasks = new Task[numberOfThreads];
            for (var i = 0; i < numberOfThreads; i++)
            {
                var matcher = new Broker(this.companies, this.context, this.name, this.period);
                this.tasks[i] = Task.Factory.StartNew(matcher.Run);
            }
        }

        public void Wait()
        {
            Task.WaitAll(this.tasks);
        }
    }
}

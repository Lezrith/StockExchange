using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Data;

namespace BrokerLogic
{
    /// <summary>
    /// Starts its own thread and creates orders forever.
    /// </summary>
    public class BrokerManager
    {
        private readonly Broker broker;
        private readonly Thread brokerThread;

        public BrokerManager(ICollection<string> companies, CassandraContext context, string name, TimeSpan period)
        {
            this.broker = new Broker(companies, context, name, period);
            this.brokerThread = new Thread(this.broker.Run);
            this.brokerThread.Start();
        }
    }
}

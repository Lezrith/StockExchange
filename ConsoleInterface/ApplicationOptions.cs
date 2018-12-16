using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleInterface
{
    public class ApplicationOptions
    {
        public IList<string> Companies { get; set; }
        public int NumberOfMatchers { get; set; }
        public int NumberOfBrokers { get; set; }
        public double MatcherWaitTime { get; set; }
        public double BrokerWaitTime { get; set; }
        public bool UseConsistencyMonitor { get; set; }
        public double ConsistencyMonitorWaitTime { get; set; }
    }
}

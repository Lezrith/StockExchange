using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data;

namespace MatcherLogic
{
    public class MatcherManager
    {
        public MatcherManager(ICollection<string> companies, CassandraContext context, TimeSpan period, int numberOfThreads)
        {
            var tasks = new Task[numberOfThreads];
            for (var i = 0; i < numberOfThreads; i++)
            {
                var matcher = new Matcher(companies, context, new Random(), period);
                tasks[i] = Task.Factory.StartNew(matcher.Run);
            }

            Task.WaitAll(tasks);
        }
    }
}

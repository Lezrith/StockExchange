using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;

namespace Logic.Matcher
{
    public class MatcherManager : IManager
    {
        private readonly ICollection<string> companies;
        private readonly CassandraContext context;
        private readonly TimeSpan period;
        private Task[] tasks;

        public MatcherManager(ICollection<string> companies, CassandraContext context, TimeSpan period)
        {
            this.period = period;
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.companies = companies ?? throw new ArgumentNullException(nameof(companies));
        }

        public void Start(int numberOfThreads)
        {
            this.tasks = new Task[numberOfThreads];
            for (var i = 0; i < numberOfThreads; i++)
            {
                var matcher = new Matcher(this.companies, this.context, new Random(), this.period);
                this.tasks[i] = Task.Factory.StartNew(matcher.Run);
            }
        }

        public void Wait()
        {
            Task.WaitAll(this.tasks);
        }
    }
}

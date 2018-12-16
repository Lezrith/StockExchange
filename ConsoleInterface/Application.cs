using System;
using Data;
using Logic;
using Logic.Broker;
using Logic.Matcher;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleInterface
{
    internal class Application : IApplication
    {
        private readonly ApplicationOptions options;
        private readonly ILogger logger;
        private readonly CassandraContext context;

        public Application(IOptions<ApplicationOptions> options, ILogger<Application> logger, CassandraContext context)
        {
            this.options = options.Value ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Run()
        {
            var matcherPeriod = TimeSpan.FromMilliseconds(this.options.MatcherWaitTime);
            var brokerPeriod = TimeSpan.FromMilliseconds(this.options.BrokerWaitTime);

            var matcherManager = new MatcherManager(this.options.Companies, this.context, matcherPeriod);
            var brokerManager = new BrokerManager(this.options.Companies, this.context, "broker", brokerPeriod);

            matcherManager.Start(this.options.NumberOfMatchers);
            brokerManager.Start(this.options.NumberOfBrokers);
            
            this.logger.LogInformation($"cql version {this.context.GetCqlVersion()}; app started");

            if (this.options.UseConsistencyMonitor)
            {
                var monitorPeriod = TimeSpan.FromMilliseconds(this.options.ConsistencyMonitorWaitTime);
                var monitor = new ConsistencyMonitor(this.options.Companies, this.context, this.logger, monitorPeriod);
                monitor.Start();
            }
            else
            {
                matcherManager.Wait();
                brokerManager.Wait();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Logic.Broker;
using Logic.Matcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleInterface
{
    internal class Application : IApplication
    {
        private readonly IConfigurationRoot configuration;
        private readonly ILogger logger;
        private readonly CassandraContext context;

        public Application(IConfigurationRoot configuration, ILogger<Application> logger, CassandraContext context)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Run()
        {
            var companies = this.configuration.GetSection("Logic:Companies").Get<IEnumerable<string>>().ToList();
            var numberOfBrokers = this.configuration.GetSection("Logic:NumberOfBrokers").Get<int>();
            var numberOfMatchers = this.configuration.GetSection("Logic:NumberOfMatchers").Get<int>();

            var matcherManager = new MatcherManager(companies, this.context, TimeSpan.FromSeconds(1));
            var brokerManager = new BrokerManager(companies, this.context, "broker", TimeSpan.FromSeconds(2));

            matcherManager.Start(numberOfMatchers);
            brokerManager.Start(numberOfBrokers);
            matcherManager.Wait();
            brokerManager.Wait();
        }
    }
}

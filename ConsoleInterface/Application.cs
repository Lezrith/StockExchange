using System;
using System.Collections.Generic;
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
            this.logger.LogDebug($"hello from cql version {this.context.GetCqlVersion()}");

            var companies = new List<string> { "Apple", "Intel", "Microsoft" }; // TODO extract it from the configuration file
            var matcherManager = new MatcherManager(companies, this.context, TimeSpan.FromSeconds(1));
            var brokerManager = new BrokerManager(companies, this.context, "krzysztof", TimeSpan.FromSeconds(2));

            matcherManager.Start(1);
            brokerManager.Start(1);
            matcherManager.Wait();
            brokerManager.Wait();
        }
    }
}

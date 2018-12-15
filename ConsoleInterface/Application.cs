using System;
using System.Collections.Generic;
using System.Text;
using BrokerLogic;
using Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleInterface
{
    internal class Application : IApplication
    {
        private readonly IConfigurationRoot configuration;
        private readonly ILogger logger;
        private readonly CassandraContext context;
        private BrokerManager brokerManager;

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
            this.brokerManager = new BrokerManager(companies, this.context, "krzysztof", TimeSpan.FromSeconds(2));
        }
    }
}

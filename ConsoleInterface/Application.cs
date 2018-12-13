using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleInterface
{
    internal class Application : IApplication
    {
        private readonly IConfigurationRoot configuration;
        private readonly ILogger logger;

        public Application(IConfigurationRoot configuration, ILogger<Application> logger)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Run()
        {
            this.logger.LogDebug($"hello {this.configuration["Hello"]}");
        }
    }
}

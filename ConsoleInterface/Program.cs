using System;
using System.IO;
using Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleInterface
{
    internal static class Program
    {
        private static IConfigurationRoot LoadConfiguration()
        {
            string file;
#if DEBUG
            file = "appsettings.Development.json";
#else
            file = "appsettings.Release.json";
#endif
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(file, optional: false, reloadOnChange: true);

            return builder.Build();
        }

        private static ServiceProvider ConfigureServices()
        {
            var configuration = LoadConfiguration();

            return new ServiceCollection()
                .AddLogging(logging =>
                {
                    logging.AddConfiguration(configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .AddSingleton(configuration)
                .Configure<CassandraOptions>(configuration.GetSection("Cassandra"))
                .Configure<ApplicationOptions>(configuration.GetSection("Application"))
                .AddSingleton<CassandraContext>()
                .AddSingleton<IApplication, Application>()
                .BuildServiceProvider();
        }

        private static void Main()
        {
            using (var serviceProvider = ConfigureServices())
            {
                var application = serviceProvider.GetService<IApplication>();
                application.Run();
            }
        }
    }
}

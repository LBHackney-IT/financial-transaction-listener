using FinancialTransactionListener.Infrastructure;
using Hackney.Core.DynamoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;

namespace FinancialTransactionListener.Tests
{


    public class MockApplicationFactory
    {
        public IElasticClient ElasticSearchClient { get; private set; }

        public MockApplicationFactory()
        { }

        public IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IConfiguration>(hostContext.Configuration);
                services.ConfigureElasticSearch(hostContext.Configuration);
                services.ConfigureDynamoDB();
                var serviceProvider = services.BuildServiceProvider();
                ElasticSearchClient = serviceProvider.GetRequiredService<IElasticClient>();
            });
    }
}

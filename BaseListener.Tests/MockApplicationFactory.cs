using Hackney.Core.DynamoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BaseListener.Tests
{
    public class MockApplicationFactory
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public MockApplicationFactory()
        { }

        public IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               services.ConfigureDynamoDB();

               ServiceProvider = services.BuildServiceProvider();
           });
    }
}

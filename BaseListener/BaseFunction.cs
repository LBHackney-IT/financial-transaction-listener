using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using BaseListener.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace BaseListener
{
    /// <summary>
    /// Base class used for all functions
    /// Sets up the necessary DI container for the function.
    /// Any function-specific registration and configuration should be done in the derived class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BaseFunction
    {
        protected readonly static JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        protected IConfigurationRoot Configuration { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected ILogger Logger { get; }

        internal BaseFunction()
        {
            AWSSDKHandler.RegisterXRayForAllServices();

            var services = new ServiceCollection();
            var builder = new ConfigurationBuilder();

            Configure(builder);
            Configuration = builder.Build();
            services.AddSingleton<IConfiguration>(Configuration);

            services.ConfigureLambdaLogging(Configuration);
            services.AddLogCallAspect();

            ConfigureServices(services);

            // TODO - Remove if not using DynamoDb
            if (Configuration.GetValue<bool>("DynamoDb_LocalMode"))
                AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.UseLogCall();

            Logger = ServiceProvider.GetRequiredService<ILogger<BaseFunction>>();
        }

        /// <summary>
        /// Base implementation
        /// Automatically adds environment variables and the appsettings file
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Configure(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            if (!string.IsNullOrEmpty(environment))
            {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"appsettings.{environment}.json");
                if (File.Exists(path))
                    builder.AddJsonFile(path);
            }
            builder.AddEnvironmentVariables();
        }

        /// <summary>>
        /// Base implementation
        /// Automatically adds LogCallAspect
        /// </summary>
        /// <param name="services"></param>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddLogCallAspect();
        }
    }
}

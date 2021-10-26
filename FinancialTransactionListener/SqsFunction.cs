using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FinancialTransactionListener.Boundary;
using FinancialTransactionListener.Gateway;
using FinancialTransactionListener.Gateway.Interfaces;
using FinancialTransactionListener.Infrastructure;
using FinancialTransactionListener.UseCase;
using FinancialTransactionListener.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FinancialTransactionListener
{
    /// <summary>
    /// Lambda function triggered by an SQS message
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SqsFunction : BaseFunction
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public SqsFunction()
        { }

        /// <summary>
        /// Use this method to perform any DI configuration required
        /// </summary>
        /// <param name="services"></param>
        protected override void ConfigureServices(IServiceCollection services)
        {
            //services.ConfigureDynamoDB();

            services.AddHttpClient();

            services.AddScoped<IEsGateway, EsGateway>();
            services.AddScoped<ITransactionApiGateway, TransactionApiGateway>();
            services.AddScoped<IIndexTransactionUseCase, IndexTransactionUseCase>();
            services.ConfigureElasticSearch(Configuration);
            base.ConfigureServices(services);
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            // Do this in parallel???
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method called to process every distinct message received.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
       // [LogCall(LogLevel.Information)]
        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message {message.MessageId}");

            var entityEvent = JsonSerializer.Deserialize<EntityEventSns>(message.Body, JsonOptions);

            if (entityEvent != null)
                using (Logger.BeginScope("CorrelationId: {CorrelationId}", entityEvent.CorrelationId))
                {
                    try
                    {
                        IMessageProcessing processor;
                        switch (entityEvent.EventType)
                        {
                            case EventTypes.TransactionCreatedEvent:
                            case EventTypes.TransactionUpdatedEvent:
                                {
                                    processor = ServiceProvider.GetService<IIndexTransactionUseCase>();
                                    break;
                                }
                            // TODO - Implement other message types here...
                            default:
                                throw new ArgumentException(
                                    $"Unknown event type: {entityEvent.EventType} on message id: {message.MessageId}");
                        }

                        await processor.ProcessMessageAsync(entityEvent).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex,
                            $"Exception processing message id: {message.MessageId}; type: {entityEvent.EventType}; entity id: {entityEvent.EntityId}");
                        throw; // AWS will handle retry/moving to the dead letter queue
                    }
                }
        }
    }
}

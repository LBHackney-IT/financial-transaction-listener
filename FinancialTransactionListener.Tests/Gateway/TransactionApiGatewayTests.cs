using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.Gateway;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace FinancialTransactionListener.Tests.Gateway
{
    [Collection("LogCall collection")]
    public class TransactionApiGatewayTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly TransactionApiGateway _sut;
        private IConfiguration _configuration;
        private static readonly JsonSerializerOptions _jsonOptions = global::FinancialTransactionListener.V1.Infrastructure.JsonOptions.CreateJsonOptions();

        private const string TransactionApiRoute = "https://some-domain.com/api/";
        private const string TransactionApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public TransactionApiGatewayTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                                  .Returns(httpClient);

            var inMemorySettings = new Dictionary<string, string> {
                { "TransactionApiUrl", TransactionApiRoute },
                { "TransactionApiToken", TransactionApiToken }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new TransactionApiGateway(_mockHttpClientFactory.Object, _configuration);
        }

        private static string Route(Guid id) => $"{TransactionApiRoute}Transactions/{id}";

        private static bool ValidateRequest(string expectedRoute, HttpRequestMessage request)
        {
            return (request.RequestUri.ToString() == expectedRoute)
                && (request.Headers.Authorization.ToString() == TransactionApiToken);
        }

        private void SetupHttpClientResponse(string route, Transaction response)
        {
            var statusCode = (response is null) ?
                HttpStatusCode.NotFound : HttpStatusCode.OK;
            HttpContent content = (response is null) ?
                null : new StringContent(JsonSerializer.Serialize(response, _jsonOptions));
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => ValidateRequest(route, y)),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = statusCode,
                       Content = content,
                   });
        }

        private void SetupHttpClientErrorResponse(string route, string response)
        {
            HttpContent content = (response is null) ? null : new StringContent(response);
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.InternalServerError,
                       Content = content,
                   });
        }

        private void SetupHttpClientException(string route, Exception ex)
        {
            _mockHttpMessageHandler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.Is<HttpRequestMessage>(y => y.RequestUri.ToString() == route),
                        ItExpr.IsAny<CancellationToken>())
                   .ThrowsAsync(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("sdrtgdfstg")]
        public void ConstructorTestInvalidRouteConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "TransactionApiUrl", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new TransactionApiGateway(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ConstructorTestInvalidTokenConfigThrows(string invalidValue)
        {
            var inMemorySettings = new Dictionary<string, string> {
                { "TransactionApiUrl", TransactionApiRoute },
                { "TransactionApiToken", invalidValue }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action act = () => _ = new TransactionApiGateway(_mockHttpClientFactory.Object, _configuration);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetTransactionByIdAsyncGetExceptionThrown()
        {
            var id = Guid.NewGuid();
            const string exMessage = "This is an exception";
            SetupHttpClientException(Route(id), new Exception(exMessage));

            Func<Task<Transaction>> func =
                async () => await _sut.GetTransactionByIdAsync(id).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public void GetTransactionByIdAsyncCallFailedExceptionThrown()
        {
            var id = Guid.NewGuid();
            const string error = "This is an error message";
            SetupHttpClientErrorResponse(Route(id), error);

            Func<Task<Transaction>> func =
                async () => await _sut.GetTransactionByIdAsync(id).ConfigureAwait(false);

            func.Should().ThrowAsync<GetTransactionException>()
                         .WithMessage($"Failed to get transaction details for id {id}. " +
                         $"Status code: {HttpStatusCode.InternalServerError}; Message: {error}");
        }

        [Fact]
        public async Task GetTransactionByIdAsyncNotFoundReturnsNull()
        {
            var id = Guid.NewGuid();
            SetupHttpClientResponse(Route(id), null);

            var result = await _sut.GetTransactionByIdAsync(id).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTransactionByIdAsyncCallReturnsTransaction()
        {
            var id = Guid.NewGuid();
            var transaction = new Fixture().Create<Transaction>();
            SetupHttpClientResponse(Route(id), transaction);

            var result = await _sut.GetTransactionByIdAsync(id).ConfigureAwait(false);

            result.Should().BeEquivalentTo(transaction);
        }
    }
}

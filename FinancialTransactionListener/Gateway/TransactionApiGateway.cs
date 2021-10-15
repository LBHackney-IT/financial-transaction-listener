using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.Gateway.Interfaces;
using FinancialTransactionListener.Infrastructure;
using FinancialTransactionListener.Infrastructure.Exceptions;
using FinancialTransactionListener.V1.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;

namespace FinancialTransactionListener.Gateway
{
    public class TransactionApiGateway : ITransactionApiGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _getTransactionApiRoute;
        private readonly string _getTransactionApiToken;

        private const string TransactionApiUrl = "TransactionApiUrl";
        private const string TransactionApiToken = "TransactionApiToken";
        private static readonly JsonSerializerOptions _jsonOptions = JsonOptions.CreateJsonOptions();

        public TransactionApiGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _getTransactionApiRoute = configuration.GetValue<string>(TransactionApiUrl)?.TrimEnd('/');
            if (string.IsNullOrEmpty(_getTransactionApiRoute) || !Uri.IsWellFormedUriString(_getTransactionApiRoute, UriKind.Absolute))
            {
                throw new ArgumentException($"Configuration does not contain a setting value for the key {TransactionApiUrl}.");
            }

            _getTransactionApiToken = configuration.GetValue<string>(TransactionApiToken);
            if (string.IsNullOrEmpty(_getTransactionApiToken))
            {
                throw new ArgumentException($"Configuration does not contain a setting value for the key {TransactionApiToken}.");
            }
        }

        [LogCall]
        public async Task<Transaction> GetTransactionByIdAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var getTransactionApiRoute = $"{_getTransactionApiRoute}/transactions/{id}";

            //client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(_getTransactionApiToken);
            client.DefaultRequestHeaders.Add("x-api-key", _getTransactionApiToken);
            var response = await RetryService.DoAsync(client.GetAsync(new Uri(getTransactionApiRoute)), 2, TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound)
            {
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<Transaction>(responseBody, _jsonOptions);
            }

            throw new GetTransactionException(id, response.StatusCode, responseBody);
        }
    }
}

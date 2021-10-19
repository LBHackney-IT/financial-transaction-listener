using Elasticsearch.Net;
using FinancialTransactionListener.Domain.Transaction;
using Microsoft.Extensions.Hosting;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FinancialTransactionListener.Tests
{
    public class ElasticSearchFixture
    {
        public IElasticClient ElasticSearchClient => _factory?.ElasticSearchClient;

        private readonly MockApplicationFactory _factory;
        private readonly IHost _host;

        private static readonly string _indexNameTransactions = "transactions";
        private static readonly Dictionary<string, string> _indexes = new Dictionary<string, string>
        {
            { _indexNameTransactions, "data/indexes/transactions.json" }
        };

        public ElasticSearchFixture()
        {
            EnsureEnvVarConfigured("ELASTICSEARCH_DOMAIN_URL", "http://localhost:9200");

            EnsureEnvVarConfigured("PersonApiUrl", FixtureConstants.TransactionApiRoute);
            EnsureEnvVarConfigured("PersonApiToken", FixtureConstants.TransactionApiToken);

            _factory = new MockApplicationFactory();
            _host = _factory.CreateHostBuilder(null).Build();
            _host.Start();

            WaitForEsInstance(ElasticSearchClient);
            EnsureIndexesExist(ElasticSearchClient);

            LogCallAspectFixture.SetupLogCallAspect();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;
            if (null != _host)
            {
                _host.StopAsync().GetAwaiter().GetResult();
                _host.Dispose();
            }
            _disposed = true;
        }

        private static void EnsureIndexesExist(IElasticClient elasticSearchClient)
        {
            foreach (var (key, value) in _indexes)
            {
                elasticSearchClient.Indices.Delete(key);

                var indexDoc = File.ReadAllTextAsync(value).Result;
                elasticSearchClient.LowLevel.Indices.CreateAsync<BytesResponse>(key, indexDoc)
                                                    .ConfigureAwait(true);
            }
        }

        private static void WaitForEsInstance(IElasticClient elasticSearchClient)
        {
            var esNodes = string.Join(';', elasticSearchClient.ConnectionSettings.ConnectionPool.Nodes.Select(x => x.Uri));
            Console.WriteLine($"ElasticSearch client using {esNodes}");

            Exception ex = null;
            var timeout = DateTime.UtcNow.AddSeconds(5); // 5 second timeout (make configurable?)
            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    var pingResponse = elasticSearchClient.Ping();
                    if (pingResponse.IsValid)
                        return;
                    else
                        ex = pingResponse.OriginalException;
                }
                catch (Exception e)
                {
                    ex = e;
                }

                Thread.Sleep(200);
            }

            if (ex != null)
                throw ex;
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        public static void GivenATransactionIsNotIndexed(Transaction transaction)
        {
            // Nothing to do here
        }


        public async Task GivenATransactionIsIndexedWithDifferentInfo(Transaction transaction)
        {
            var request = new IndexRequest<Transaction>(transaction, _indexNameTransactions);
            await ElasticSearchClient.IndexAsync(request).ConfigureAwait(false);
        }


    }

    [CollectionDefinition("ElasticSearch collection", DisableParallelization = true)]
    public class ElasticSearchCollection : ICollectionFixture<ElasticSearchFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}

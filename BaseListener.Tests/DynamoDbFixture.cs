using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BaseListener.Tests
{
    // TODO - Remove if DynamoDb is not required

    /// <summary>
    /// Class used to set up a real local DynamoDb instance so that it can be used by Gateway and E2E tests
    /// </summary>
    public class DynamoDbFixture
    {
        private readonly MockApplicationFactory _factory;
        private readonly IHost _host;

        public IAmazonDynamoDB DynamoDb { get; private set; }
        public IDynamoDBContext DynamoDbContext { get; private set; }

        private readonly List<TableDef> _tables = new List<TableDef>
        {
            // Define all tables required by the application here.
            // The definition should be exactly the same as that used in real deployed environments
            new TableDef { Name = "SomeTable", KeyName = "id", KeyType = ScalarAttributeType.S }
        };

        public DynamoDbFixture()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");

            _factory = new MockApplicationFactory();
            _host = _factory.CreateHostBuilder(null).Build();
            _host.Start();

            DynamoDb = _factory.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
            DynamoDbContext = _factory.ServiceProvider.GetRequiredService<IDynamoDBContext>();

            LogCallAspectFixture.SetupLogCallAspect();

            EnsureTablesExist(DynamoDb, _tables);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != _host)
                {
                    _host.StopAsync().GetAwaiter().GetResult();
                    _host.Dispose();
                }
                _disposed = true;
            }
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        private static void EnsureTablesExist(IAmazonDynamoDB dynamoDb, List<TableDef> tables)
        {
            foreach (var table in tables)
            {
                try
                {
                    var keySchema = new List<KeySchemaElement> { new KeySchemaElement(table.KeyName, KeyType.HASH) };
                    var attributes = new List<AttributeDefinition> { new AttributeDefinition(table.KeyName, table.KeyType) };
                    if (!string.IsNullOrEmpty(table.RangeKeyName))
                    {
                        keySchema.Add(new KeySchemaElement(table.RangeKeyName, KeyType.RANGE));
                        attributes.Add(new AttributeDefinition(table.RangeKeyName, table.RangeKeyType));
                    }

                    foreach (var localIndex in table.LocalSecondaryIndexes)
                    {
                        var indexRangeKey = localIndex.KeySchema.FirstOrDefault(y => y.KeyType == KeyType.RANGE);
                        if ((null != indexRangeKey) && (!attributes.Any(x => x.AttributeName == indexRangeKey.AttributeName)))
                            attributes.Add(new AttributeDefinition(indexRangeKey.AttributeName, ScalarAttributeType.S)); // Assume a string for now.
                    }

                    foreach (var globalIndex in table.GlobalSecondaryIndexes)
                    {
                        foreach (var key in globalIndex.KeySchema)
                        {
                            if (!attributes.Any(x => x.AttributeName == key.AttributeName))
                                attributes.Add(new AttributeDefinition(key.AttributeName, ScalarAttributeType.S)); // Assume a string for now.
                        }
                    }

                    var request = new CreateTableRequest(table.Name,
                        keySchema,
                        attributes,
                        new ProvisionedThroughput(3, 3))
                    {
                        LocalSecondaryIndexes = table.LocalSecondaryIndexes,
                        GlobalSecondaryIndexes = table.GlobalSecondaryIndexes
                    };
                    _ = dynamoDb.CreateTableAsync(request).GetAwaiter().GetResult();
                }
                catch (ResourceInUseException)
                {
                    // It already exists :-)
                }
            }
        }
    }

    public class TableDef
    {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public ScalarAttributeType KeyType { get; set; }
        public string RangeKeyName { get; set; }
        public ScalarAttributeType RangeKeyType { get; set; }
        public List<LocalSecondaryIndex> LocalSecondaryIndexes { get; set; } = new List<LocalSecondaryIndex>();
        public List<GlobalSecondaryIndex> GlobalSecondaryIndexes { get; set; } = new List<GlobalSecondaryIndex>();
    }

    [CollectionDefinition("DynamoDb collection", DisableParallelization = true)]
    public class DynamoDbCollection : ICollectionFixture<DynamoDbFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}

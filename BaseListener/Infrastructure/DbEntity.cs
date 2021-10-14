using Amazon.DynamoDBv2.DataModel;
using Hackney.Core.DynamoDb.Converters;
using System;

namespace BaseListener.Infrastructure
{
    // TODO - Alter if DynamoDb is not required

    [DynamoDBTable("SomeTable", LowerCamelCaseProperties = true)]
    public class DbEntity
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public string Description { get; set; }

        // DynamoDb documents that get modified will have a version number to ensure data integrity
        [DynamoDBVersion]
        public int? VersionNumber { get; set; }
    }
}

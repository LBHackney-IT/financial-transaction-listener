using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinancialTransactionListener.V1.Infrastructure
{
    public static class JsonOptions
    {
        public static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}

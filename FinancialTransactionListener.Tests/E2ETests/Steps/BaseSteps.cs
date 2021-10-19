using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinancialTransactionListener.Tests.E2ETests.Steps
{
    public class BaseSteps
    {
        protected readonly JsonSerializerOptions JsonOptions;
        protected readonly List<Action> _cleanup = new List<Action>();

        protected BaseSteps()
        {
            JsonOptions = CreateJsonOptions();
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;
            foreach (var action in _cleanup)
                action();

            _disposed = true;
        }
    }
}

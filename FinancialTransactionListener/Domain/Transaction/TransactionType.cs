using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinancialTransactionListener.V1.Domain.Transaction
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
    {
        Rent,
        Charge
    }
}

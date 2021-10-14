using System;
using System.Net;

namespace FinancialTransactionListener.Infrastructure.Exceptions
{
    public class GetTransactionException : Exception
    {
        public Guid TransactionId { get; }
        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }

        public GetTransactionException(Guid id, HttpStatusCode statusCode, string responseBody)
            : base($"Failed to get person details for id {id}. Status code: {statusCode}; Message: {responseBody}")
        {
            TransactionId = id;
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}

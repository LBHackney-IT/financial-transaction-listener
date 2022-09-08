using System;

namespace FinancialTransactionListener.Infrastructure.Exceptions
{
    public class MissedEntityDataException : ArgumentException
    {
        public MissedEntityDataException(string message)
            : base(message) { }
    }
}

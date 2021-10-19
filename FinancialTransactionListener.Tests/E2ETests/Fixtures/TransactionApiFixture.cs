using System;
using AutoFixture;
using FinancialTransactionListener.Domain.Transaction;

namespace FinancialTransactionListener.Tests.E2ETests.Fixtures
{
    public class TransactionApiFixture : BaseApiFixture<Transaction>
    {
        public TransactionApiFixture()
        {
            Environment.SetEnvironmentVariable("TransactionApiUrl", FixtureConstants.TransactionApiRoute);
            Environment.SetEnvironmentVariable("TransactionApiToken", FixtureConstants.TransactionApiToken);

            Route = FixtureConstants.TransactionApiRoute;
            Token = FixtureConstants.TransactionApiToken;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing || Disposed) return;
            ResponseObject = null;
            base.Dispose(true);
        }

        public static void GivenTheTransactionDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public Transaction GivenTheTransactionExists(Guid id)
        {
            ResponseObject = Fixture.Build<Transaction>()
                                     .With(x => x.Id, id)
                                     .Create();
            return ResponseObject;
        }
    }
}

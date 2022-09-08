using System;
using AutoFixture;
using FinancialTransactionListener.Domain.Transaction;
using Hackney.Core.Testing.Shared.E2E;

namespace FinancialTransactionListener.Tests.E2ETests.Fixtures
{
    public class TransactionApiFixture : BaseApiFixture<Transaction>
    {
        private readonly Fixture _fixture = new Fixture();
        public TransactionApiFixture() : base(FixtureConstants.TransactionApiRoute, FixtureConstants.TransactionApiToken)
        {
            Environment.SetEnvironmentVariable("TransactionApiUrl", FixtureConstants.TransactionApiRoute);
            Environment.SetEnvironmentVariable("TransactionApiToken", FixtureConstants.TransactionApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        public void GivenTheTransactionDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public Transaction GivenTheTransactionExists(Guid id)
        {
            ResponseObject = _fixture.Build<Transaction>()
                                     .With(x => x.Id, id)
                                     .Create();
            return ResponseObject;
        }
    }
}

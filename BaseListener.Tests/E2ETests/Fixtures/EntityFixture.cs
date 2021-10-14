using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using BaseListener.Infrastructure;
using System;

namespace BaseListener.Tests.E2ETests.Fixtures
{
    public class EntityFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly IDynamoDBContext _dbContext;

        public DbEntity DbEntity { get; private set; }
        public Guid DbEntityId { get; private set; }

        public EntityFixture(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != DbEntity)
                    _dbContext.DeleteAsync<DbEntity>(DbEntity.Id).GetAwaiter().GetResult();

                _disposed = true;
            }
        }

        private DbEntity ConstructAndSaveEntity(Guid id)
        {
            var dbEntity = _fixture.Build<DbEntity>()
                                 .With(x => x.Id, id)
                                 .With(x => x.VersionNumber, (int?) null)
                                 .Create();

            _dbContext.SaveAsync<DbEntity>(dbEntity).GetAwaiter().GetResult();
            dbEntity.VersionNumber = 0;
            return dbEntity;
        }

        public void GivenAnEntityAlreadyExists(Guid id)
        {
            if (null == DbEntity)
            {
                var entity = ConstructAndSaveEntity(id);
                DbEntity = entity;
                DbEntityId = entity.Id;
            }
        }

        public void GivenAnEntityDoesNotExist(Guid id)
        {
            // Nothing to do here
        }
    }
}

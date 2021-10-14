using BaseListener.Domain;
using BaseListener.Infrastructure;

namespace BaseListener.Factories
{
    public static class EntityFactory
    {
        public static DomainEntity ToDomain(this DbEntity databaseEntity)
        {
            return new DomainEntity
            {
                // TODO - Implement factory method fully
                Id = databaseEntity.Id,
                Name = databaseEntity.Name,
                Description = databaseEntity.Description,
                VersionNumber = databaseEntity.VersionNumber
            };
        }

        public static DbEntity ToDatabase(this DomainEntity entity)
        {
            return new DbEntity
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                VersionNumber = entity.VersionNumber
            };
        }
    }
}

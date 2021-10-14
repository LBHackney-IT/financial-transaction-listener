using System;

namespace BaseListener.Infrastructure.Exceptions
{
    public class EntityNotFoundException<T> : Exception where T : class
    {
        public string EntityName => typeof(T).Name;
        public Guid Id { get; }

        public EntityNotFoundException(Guid id)
            : base($"{typeof(T).Name} with id {id} not found.")
        {
            Id = id;
        }
    }
}

using AspireApp.Domain.Entities.Base;

namespace AspireApp.Tests.Client.Extensions;

public static class EntityExtensions
{
    public static TID SetId<TEntity, TID>(this TEntity entity) where TEntity : class
                                                               where TID : struct
    {
        var newId = Activator.CreateInstance<TID>();
        var propertyId = entity.GetType().GetProperty(nameof(BaseEntity<TID>.Id));

        if (propertyId is not null)
        {
            if (newId is Guid)
                newId = (TID)Convert.ChangeType(Guid.NewGuid(), typeof(TID));

            else if (newId is int || newId is long)
                newId = (TID)Convert.ChangeType(1, typeof(TID));

            propertyId?.SetValue(entity, newId);
        }

        return newId;
    }
}

namespace NekoViBE.Domain.Common;

public static class EntityExtension
{
    public static void InitializeEnitity(this IEntityLike entity, Guid? userId = null)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = userId ?? Guid.Empty; //Guid.Empty is for system
    }

    public static void UpdateEnitity(this IEntityLike entity, IEntityLike? oldEntity = null, Guid? userId = null)
    {
        if (oldEntity != null)
        {
            entity.CreatedAt = oldEntity.CreatedAt;
            entity.CreatedBy = oldEntity.CreatedBy;
        }
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId ?? Guid.Empty;
    }

    public static void SoftDeleteEnitity(this IEntityLike entity, Guid? userId = null)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            baseEntity.DeletedAt = DateTime.UtcNow;
            baseEntity.DeletedBy = userId ?? Guid.Empty;
        }
    }

    public static void RestoreEnitity(this IEntityLike entity, Guid? userId = null)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = false;
            baseEntity.DeletedAt = null;
            baseEntity.DeletedBy = null;
        }
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId ?? Guid.Empty;
    }
}
using Microsoft.Extensions.DependencyInjection;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Common.Helpers;

/// <summary>
/// Global helper for creating and logging user actions
/// Eliminates duplicate code across command handlers
/// </summary>
public static class UserActionHelper
{
    /// <summary>
    /// Creates and logs a user action asynchronously (fire and forget)
    /// </summary>
    /// <param name="serviceProvider">Service provider for scoped dependencies</param>
    /// <param name="userId">User ID performing the action</param>
    /// <param name="action">Type of action (Create, Update, Delete)</param>
    /// <param name="entityId">ID of the entity being acted upon</param>
    /// <param name="entityName">Name of the entity type</param>
    /// <param name="actionDetail">Description of the action</param>
    /// <param name="ipAddress">IP address of the user</param>
    /// <param name="oldValue">Old value (for Update/Delete operations)</param>
    /// <param name="newValue">New value (for Create/Update operations)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static void LogUserActionAsync(
        IServiceProvider serviceProvider,
        Guid userId,
        UserActionEnum action,
        Guid entityId,
        string entityName,
        string actionDetail,
        string? ipAddress,
        object? oldValue = null,
        object? newValue = null,
        CancellationToken cancellationToken = default)
    {
        // Fire and forget pattern - don't block the main operation
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var userAction = new UserAction
                {
                    UserId = userId,
                    Action = action,
                    EntityId = entityId,
                    EntityName = entityName,
                    OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                    NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null,
                    IPAddress = ipAddress,
                    ActionDetail = actionDetail,
                };
                
                userAction.InitializeEntity(userId);
                await unitOfWork.Repository<UserAction>().AddAsync(userAction);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - this is a background operation
                // Consider using ILogger here if needed
                Console.WriteLine($"Failed to log user action: {ex.Message}");
            }
        }, cancellationToken);
    }
    
    /// <summary>
    /// Creates a user action for Create operation
    /// </summary>
    public static void LogCreateAction<TEntity>(
        IServiceProvider serviceProvider,
        Guid userId,
        Guid entityId,
        TEntity entity,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        LogUserActionAsync(
            serviceProvider,
            userId,
            UserActionEnum.Create,
            entityId,
            typeof(TEntity).Name,
            $"{typeof(TEntity).Name} created",
            ipAddress,
            oldValue: null,
            newValue: entity,
            cancellationToken
        );
    }
    
    /// <summary>
    /// Creates a user action for Update operation
    /// </summary>
    public static void LogUpdateAction<TEntity>(
        IServiceProvider serviceProvider,
        Guid userId,
        Guid entityId,
        TEntity oldEntity,
        TEntity newEntity,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        LogUserActionAsync(
            serviceProvider,
            userId,
            UserActionEnum.Update,
            entityId,
            typeof(TEntity).Name,
            $"{typeof(TEntity).Name} updated",
            ipAddress,
            oldValue: oldEntity,
            newValue: newEntity,
            cancellationToken
        );
    }
    
    /// <summary>
    /// Creates a user action for Delete operation
    /// </summary>
    public static void LogDeleteAction<TEntity>(
        IServiceProvider serviceProvider,
        Guid userId,
        Guid entityId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        LogUserActionAsync(
            serviceProvider,
            userId,
            UserActionEnum.Delete,
            entityId,
            typeof(TEntity).Name,
            $"{typeof(TEntity).Name} deleted",
            ipAddress,
            oldValue: null,
            newValue: null,
            cancellationToken
        );
    }
}


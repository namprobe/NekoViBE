using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Infrastructure.Context;

public class NekoViDbContext : IdentityDbContext<AppUser, AppRole, Guid>, INekoViDbContext
{
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<StaffProfile> StaffProfiles { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }

    public NekoViDbContext(DbContextOptions<NekoViDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Apply soft delete filter to all entities inheriting from BaseEntity
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var condition = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(condition, parameter);
                
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // Đổi tên bảng Identity
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // AppUser
        builder.Entity<AppUser>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => x.Id).IsUnique();

            // One-to-many: User -> UserAddresses
            entity
                .HasMany(u => u.UserAddresses)
                .WithOne(a => a.User!)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one: User -> CustomerProfile
            entity
                .HasOne(u => u.CustomerProfile)
                .WithOne(p => p!.User!)
                .HasForeignKey<CustomerProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one: User -> StaffProfile
            entity
                .HasOne(u => u.StaffProfile)
                .WithOne(p => p!.User!)
                .HasForeignKey<StaffProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AppRole
        builder.Entity<AppRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(x => x.Status).HasConversion<int>();
        });

        // Configure the base IdentityUserRole<Guid> key
        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
        });

        // UserAddress
        builder.Entity<UserAddress>(entity =>
        {
            entity.Property(x => x.AddressType).HasConversion<int>();
            entity.Property(x => x.IsDefault).HasDefaultValue(true);
        });

        // StaffProfile
        builder.Entity<StaffProfile>(entity =>
        {
            entity.Property(x => x.Salary).HasColumnType("decimal(18,2)");
        });

        // Unique FKs for 1:1 relations
        builder.Entity<CustomerProfile>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<StaffProfile>().HasIndex(x => x.UserId).IsUnique();

        // Gọi cấu hình chung cho BaseEntity
        ConfigureBaseEntity(builder);
    }

    private void ConfigureBaseEntity(ModelBuilder modelBuilder)
    {
        // Cấu hình chung cho tất cả BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
        {
            // Primary key
            modelBuilder.Entity(entityType.ClrType).HasKey("Id");
            
            // Default values for audit fields
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime?>("CreatedAt")
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
                
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime?>("UpdatedAt")
                .HasColumnType("datetime");
            
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime?>("DeletedAt")
                .HasColumnType("datetime");
                
            modelBuilder.Entity(entityType.ClrType)
                .Property<bool>("IsDeleted")
                .HasDefaultValue(false);

            // Enum conversions
            modelBuilder.Entity(entityType.ClrType)
                .Property<EntityStatusEnum>("Status")
                .HasConversion<int>();
            
        }
    }
}
    

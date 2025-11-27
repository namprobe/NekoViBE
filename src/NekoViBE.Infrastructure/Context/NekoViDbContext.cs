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
    public DbSet<UserAction> UserActions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AnimeSeries> AnimeSeries { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductInventory> ProductInventories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ShippingMethod> ShippingMethods { get; set; }
    public DbSet<OrderShippingMethod> OrderShippingMethods { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<UserCoupon> UserCoupons { get; set; }
    public DbSet<PostCategory> PostCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventProduct> EventProducts { get; set; }
    public DbSet<HomeImage> HomeImages { get; set; } = null!;
    public DbSet<UserHomeImage> UserHomeImages { get; set; } = null!;


    public NekoViDbContext(DbContextOptions<NekoViDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
            // One-to-many: User -> UserActions
            entity
                .HasMany(u => u.UserActions)
                .WithOne(a => a.User!)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Performance indexes
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.JoiningAt);
            entity.HasIndex(x => x.LastLoginAt).HasFilter("LastLoginAt IS NOT NULL");
            entity.HasIndex(x => new { x.Status, x.JoiningAt });
        });

        // AppRole
        builder.Entity<AppRole>(entity =>
        {
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
            
            // Performance indexes
            entity.HasIndex(x => new { x.UserId, x.IsDefault }).HasFilter("IsDefault = 1");
            entity.HasIndex(x => x.AddressType);
            entity.HasIndex(x => new { x.Country, x.State, x.City });
        });

        // StaffProfile
        builder.Entity<StaffProfile>(entity =>
        {
            entity.Property(x => x.Salary).HasColumnType("decimal(10,2)");
            
            // Performance indexes
            entity.HasIndex(x => x.Position).HasFilter("Position IS NOT NULL");
            entity.HasIndex(x => x.HireDate).HasFilter("HireDate IS NOT NULL");
            entity.HasIndex(x => x.UserId).HasFilter("LeaveDate IS NULL").HasDatabaseName("IX_StaffProfiles_Active");
            entity.HasIndex(x => x.Salary).HasFilter("Salary IS NOT NULL");
        });

        // CustomerProfile
        builder.Entity<CustomerProfile>(entity =>
        {
            // Unique FK for 1:1 relation
            entity.HasIndex(x => x.UserId).IsUnique();
            
            // Performance indexes
            entity.HasIndex(x => x.DateOfBirth).HasFilter("DateOfBirth IS NOT NULL");
            entity.HasIndex(x => x.Gender).HasFilter("Gender IS NOT NULL");
        });

        // StaffProfile unique index (already configured above with other indexes)
        builder.Entity<StaffProfile>().HasIndex(x => x.UserId).IsUnique();

        //UserAction
        builder.Entity<UserAction>(entity =>
        {
            entity.Property(x => x.Action).HasConversion<int>();
            
            // Performance indexes
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.HasIndex(x => x.EntityId).HasFilter("EntityId IS NOT NULL");
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.IPAddress).HasFilter("IPAddress IS NOT NULL");
        });
        
        //Category
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.ParentCategoryId).HasFilter("ParentCategoryId IS NOT NULL");
            entity.HasIndex(x => x.ImagePath).HasFilter("ImagePath IS NOT NULL");
            entity.HasMany(x => x.SubCategories)
                .WithOne(x => x.ParentCategory!)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        //AnimeSeries
        builder.Entity<AnimeSeries>(entity =>
        {
            entity.HasIndex(x => x.ImagePath).HasFilter("ImagePath IS NOT NULL");
            entity.HasIndex(x => x.ReleaseYear).HasFilter("ReleaseYear IS NOT NULL");
        });

        // Product
        builder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.Property(x => x.DiscountPrice).HasColumnType("decimal(10,2)");
            
            // Relationships
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.AnimeSeries)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.AnimeSeriesId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes
            entity.HasIndex(x => x.CategoryId);
            entity.HasIndex(x => x.AnimeSeriesId).HasFilter("AnimeSeriesId IS NOT NULL");
            entity.HasIndex(x => x.IsPreOrder);
            entity.HasIndex(x => x.PreOrderReleaseDate).HasFilter("PreOrderReleaseDate IS NOT NULL");
            entity.HasIndex(x => x.Price);
            entity.HasIndex(x => x.StockQuantity);
        });

        // ProductInventory
        builder.Entity<ProductInventory>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductInventories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.ProductId);
        });

        // ProductImage
        builder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductImages)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.ProductId);
            entity.HasIndex(x => x.IsPrimary);
            entity.HasIndex(x => x.DisplayOrder);
        });

        // Tag
        builder.Entity<Tag>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
        });

        // ProductTag
        builder.Entity<ProductTag>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductTags)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Tag)
                .WithMany(x => x.ProductTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => new { x.ProductId, x.TagId }).IsUnique();
        });

        // ProductReview
        builder.Entity<ProductReview>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductReviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.User)
                .WithMany(x => x.ProductReviews)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.ProductId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Rating);
        });

        // Badge
        builder.Entity<Badge>(entity =>
        {
            entity.Property(x => x.DiscountPercentage).HasColumnType("decimal(5,2)");
            entity.Property(x => x.ConditionType).HasConversion<int>();
            
            entity.HasIndex(x => x.ConditionType);
            entity.HasIndex(x => x.IsTimeLimited);
            entity.HasIndex(x => x.StartDate).HasFilter("StartDate IS NOT NULL");
            entity.HasIndex(x => x.EndDate).HasFilter("EndDate IS NOT NULL");
            entity.HasIndex(x => x.IconPath).HasFilter("IconPath IS NOT NULL");
        });

        // UserBadge
        builder.Entity<UserBadge>(entity =>
        {
            entity.HasOne(x => x.User)
                .WithMany(x => x.UserBadges)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Badge)
                .WithMany(x => x.UserBadges)
                .HasForeignKey(x => x.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.BadgeId);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.EarnedDate);
        });

        // ShoppingCart
        builder.Entity<ShoppingCart>(entity =>
        {
            entity.HasOne(x => x.User)
                .WithOne(x => x.ShoppingCart)
                .HasForeignKey<ShoppingCart>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.UserId).IsUnique();
        });

        // CartItem
        builder.Entity<CartItem>(entity =>
        {
            entity.HasOne(x => x.Cart)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Product)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.CartId);
            entity.HasIndex(x => x.ProductId);
        });

        // Order
        builder.Entity<Order>(entity =>
        {
            entity.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.DiscountAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.TaxAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.ShippingAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.FinalAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.PaymentStatus).HasConversion<int>();
            entity.Property(x => x.OrderStatus).HasConversion<int>();
            
            entity.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(x => x.UserId).HasFilter("UserId IS NOT NULL");
            entity.HasIndex(x => x.PaymentStatus);
            entity.HasIndex(x => x.OrderStatus);
            entity.HasIndex(x => x.IsOneClick);
        });

        // OrderItem
        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(x => x.DiscountAmount).HasColumnType("decimal(10,2)");
            
            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.ProductId);
        });

        // ShippingMethod
        builder.Entity<ShippingMethod>(entity =>
        {
            entity.Property(x => x.Cost).HasColumnType("decimal(10,2)");
            
            entity.HasIndex(x => x.EstimatedDays).HasFilter("EstimatedDays IS NOT NULL");
        });

        // OrderShippingMethod
        builder.Entity<OrderShippingMethod>(entity =>
        {
            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderShippingMethods)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.ShippingMethod)
                .WithMany(x => x.OrderShippingMethods)
                .HasForeignKey(x => x.ShippingMethodId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.ShippingMethodId);
            entity.HasIndex(x => x.TrackingNumber).HasFilter("TrackingNumber IS NOT NULL");
        });

        // PaymentMethod
        builder.Entity<PaymentMethod>(entity =>
        {
            entity.Property(x => x.ProcessingFee).HasColumnType("decimal(5,2)");
            
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.IsOnlinePayment);
            entity.HasIndex(x => x.ProcessorName).HasFilter("ProcessorName IS NOT NULL");
            entity.HasIndex(x => x.IconPath).HasFilter("IconPath IS NOT NULL");
        });

        // Payment (1-1 with Order)
        builder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Amount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.PaymentStatus).HasConversion<int>();
            
            entity.HasOne(x => x.Order)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.PaymentMethod)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(x => x.OrderId).IsUnique();
            entity.HasIndex(x => x.PaymentMethodId);
            entity.HasIndex(x => x.PaymentStatus);
            entity.HasIndex(x => x.TransactionNo).HasFilter("TransactionNo IS NOT NULL");
            entity.HasIndex(x => x.PaymentDate).HasFilter("PaymentDate IS NOT NULL");
        });

        // Wishlist
        builder.Entity<Wishlist>(entity =>
        {
            entity.HasOne(x => x.User)
                .WithOne(x => x.Wishlist)
                .HasForeignKey<Wishlist>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.UserId).IsUnique();
        });

        // WishlistItem
        builder.Entity<WishlistItem>(entity =>
        {
            entity.HasOne(x => x.Wishlist)
                .WithMany(x => x.WishlistItems)
                .HasForeignKey(x => x.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Product)
                .WithMany(x => x.WishlistItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.WishlistId);
            entity.HasIndex(x => x.ProductId);
        });

        // Coupon
        builder.Entity<Coupon>(entity =>
        {
            entity.Property(x => x.DiscountValue).HasColumnType("decimal(10,2)");
            entity.Property(x => x.MinOrderAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.DiscountType).HasConversion<int>();
            
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.DiscountType);
            entity.HasIndex(x => x.StartDate);
            entity.HasIndex(x => x.EndDate);
            entity.HasIndex(x => x.UsageLimit).HasFilter("UsageLimit IS NOT NULL");
        });

        // UserCoupon
        builder.Entity<UserCoupon>(entity =>
        {
            entity.HasOne(x => x.User)
                .WithMany(x => x.UserCoupons)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(x => x.Coupon)
                .WithMany(x => x.UserCoupons)
                .HasForeignKey(x => x.CouponId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Order)
                .WithMany(x => x.UserCoupons)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(x => x.UserId).HasFilter("UserId IS NOT NULL");
            entity.HasIndex(x => x.CouponId);
            entity.HasIndex(x => x.OrderId).HasFilter("OrderId IS NOT NULL");
            entity.HasIndex(x => x.UsedDate).HasFilter("UsedDate IS NOT NULL");
        });

        // PostCategory
        builder.Entity<PostCategory>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
        });

        // BlogPost
        builder.Entity<BlogPost>(entity =>
        {
            entity.HasOne(x => x.Author)
                .WithMany(x => x.BlogPosts)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(x => x.PostCategory)
                .WithMany(x => x.BlogPosts)
                .HasForeignKey(x => x.PostCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(x => x.AuthorId).HasFilter("AuthorId IS NOT NULL");
            entity.HasIndex(x => x.PostCategoryId).HasFilter("PostCategoryId IS NOT NULL");
            entity.HasIndex(x => x.PublishDate);
            entity.HasIndex(x => x.IsPublished);
        });

        // PostTag
        builder.Entity<PostTag>(entity =>
        {
            entity.HasOne(x => x.Post)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Tag)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => new { x.PostId, x.TagId }).IsUnique();
        });

        // Event
        builder.Entity<Event>(entity =>
        {
            entity.HasIndex(x => x.StartDate);
            entity.HasIndex(x => x.EndDate);
            entity.HasIndex(x => x.ImagePath).HasFilter("ImagePath IS NOT NULL");
        });

        // EventProduct
        builder.Entity<EventProduct>(entity =>
        {
            entity.Property(x => x.DiscountPercentage).HasColumnType("decimal(5,2)");
            
            entity.HasOne(x => x.Event)
                .WithMany(x => x.EventProducts)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Product)
                .WithMany(x => x.EventProducts)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => new { x.EventId, x.ProductId }).IsUnique();
            entity.HasIndex(x => x.IsFeatured);
        });

        // HomeImage
        builder.Entity<HomeImage>(entity =>
        {
            entity.Property(x => x.ImagePath).IsRequired().HasMaxLength(500);

            entity.Property(x => x.Name)
                  .IsRequired()        
                  .HasMaxLength(200);

            entity.HasIndex(x => x.AnimeSeriesId)
                  .HasFilter("AnimeSeriesId IS NOT NULL");

            entity.HasOne(x => x.AnimeSeries)
                  .WithMany()
                  .HasForeignKey(x => x.AnimeSeriesId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // UserHomeImage – bảng trung gian có thứ tự
        builder.Entity<UserHomeImage>(entity =>
        {
            entity.HasKey(x => new { x.UserId, x.HomeImageId });

            entity.HasOne(x => x.User)
                  .WithMany(u => u.SelectedHomeImages)
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.HomeImage)
                  .WithMany(h => h.UserSelections)
                  .HasForeignKey(x => x.HomeImageId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ràng buộc: mỗi user chỉ được chọn tối đa 3 ảnh
            // Position chỉ được là 1, 2, 3
            entity.Property(x => x.Position)
                  .IsRequired()
                  .HasConversion<byte>() // lưu dưới dạng TINYINT cho nhẹ
                  .HasDefaultValue(1);

            // Đảm bảo Position trong khoảng 1-3
            entity.HasCheckConstraint("CK_UserHomeImage_Position", "Position BETWEEN 1 AND 3");

            // Unique: mỗi user chỉ được chọn 1 ảnh ở 1 vị trí
            entity.HasIndex(x => new { x.UserId, x.Position })
                  .IsUnique()
                  .HasDatabaseName("UX_UserHomeImage_UserId_Position");

            // Index hỗ trợ query nhanh
            entity.HasIndex(x => x.HomeImageId);
        });

        // Gọi cấu hình chung cho BaseEntity
        BaseEntityConfigurationHelper.ConfigureBaseEntities(builder);
    }
}
    

using Microsoft.EntityFrameworkCore;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Interfaces;

public interface INekoViDbContext
{
    DbSet<CustomerProfile> CustomerProfiles { get; set; }
    DbSet<StaffProfile> StaffProfiles { get; set; }
    DbSet<UserAddress> UserAddresses { get; set; }
    DbSet<UserAction> UserActions { get; set; }
    DbSet<Category> Categories { get; set; }
    DbSet<AnimeSeries> AnimeSeries { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<ProductImage> ProductImages { get; set; }
    DbSet<Tag> Tags { get; set; }
    DbSet<ProductTag> ProductTags { get; set; }
    DbSet<ProductReview> ProductReviews { get; set; }
    DbSet<Badge> Badges { get; set; }
    DbSet<UserBadge> UserBadges { get; set; }
    DbSet<ShoppingCart> ShoppingCarts { get; set; }
    DbSet<CartItem> CartItems { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    DbSet<ShippingMethod> ShippingMethods { get; set; }
    DbSet<OrderShippingMethod> OrderShippingMethods { get; set; }
    DbSet<Payment> Payments { get; set; }
    DbSet<PaymentMethod> PaymentMethods { get; set; }
    DbSet<Wishlist> Wishlists { get; set; }
    DbSet<WishlistItem> WishlistItems { get; set; }
    DbSet<Coupon> Coupons { get; set; }
    DbSet<UserCoupon> UserCoupons { get; set; }
    DbSet<PostCategory> PostCategories { get; set; }
    DbSet<BlogPost> BlogPosts { get; set; }
    DbSet<PostTag> PostTags { get; set; }
    DbSet<Event> Events { get; set; }
    DbSet<EventProduct> EventProducts { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
using Microsoft.AspNetCore.Identity;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class AppUser : IdentityUser<Guid>, IEntityLike
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public DateTime JoiningAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? AvatarPath { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public EntityStatusEnum Status { get; set; }
    // navigation properties
    public virtual CustomerProfile? CustomerProfile { get; set; }
    public virtual StaffProfile? StaffProfile { get; set; }
    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public virtual ICollection<UserAction> UserActions { get; set; } = new List<UserAction>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    public virtual ShoppingCart? ShoppingCart { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual Wishlist? Wishlist { get; set; }
    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    // đây là trường giá sale, không phải giá trị giảm giá
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public Guid CategoryId { get; set; }
    public Guid? AnimeSeriesId { get; set; }
    public bool IsPreOrder { get; set; } = false;
    public DateTime? PreOrderReleaseDate { get; set; }
    
    // navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual AnimeSeries? AnimeSeries { get; set; }
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public virtual ICollection<EventProduct> EventProducts { get; set; } = new List<EventProduct>();
    public virtual ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
}
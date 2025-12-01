using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

/// <summary>
/// Entity lưu lịch sử các sự kiện vận chuyển (từ GHN callback hoặc các nguồn khác)
/// </summary>
public class ShippingHistory : BaseEntity
{
    /// <summary>
    /// ID của OrderShippingMethod
    /// </summary>
    public Guid OrderShippingMethodId { get; set; }
    
    /// <summary>
    /// ID của Order (để query nhanh)
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Mã tracking từ provider (GHN OrderCode)
    /// </summary>
    public string? TrackingNumber { get; set; }
    
    /// <summary>
    /// Status code từ provider (GHN status: 1-22)
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Tên status từ provider (GHN status name: "picked", "delivered", etc.)
    /// </summary>
    public string StatusName { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả tiếng Việt của status
    /// </summary>
    public string StatusDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại callback/event (GHN: "order_status", "return_status", etc.)
    /// </summary>
    public string? EventType { get; set; }
    
    /// <summary>
    /// Thời gian sự kiện xảy ra (từ provider)
    /// </summary>
    public DateTime EventTime { get; set; }
    
    /// <summary>
    /// Thông tin bổ sung từ provider (JSON string)
    /// </summary>
    public string? AdditionalData { get; set; }
    
    /// <summary>
    /// IP address của caller (nếu có)
    /// </summary>
    public string? CallerIpAddress { get; set; }
    
    // === NAVIGATION PROPERTIES ===
    public virtual OrderShippingMethod OrderShippingMethod { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
}


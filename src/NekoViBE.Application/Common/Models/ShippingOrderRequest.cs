namespace NekoViBE.Application.Common.Models;

public class ShippingOrderRequest
{
    // From address (shop)
    public string? FromName { get; set; }
    public string? FromPhone { get; set; }
    public string? FromAddress { get; set; }
    public string? FromWardName { get; set; }
    public string? FromDistrictName { get; set; }
    public string? FromProvinceName { get; set; }

    // To address (customer)
    public string ToName { get; set; } = string.Empty;
    public string ToPhone { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string ToWardName { get; set; } = string.Empty;
    public string ToDistrictName { get; set; } = string.Empty;
    public string ToProvinceName { get; set; } = string.Empty;

    // Order details
    public string? ClientOrderCode { get; set; }
    public int PaymentTypeId { get; set; } = 2; // 1: Người gửi, 2: Người nhận
    public int ServiceTypeId { get; set; } = 2; // 2: Hàng nhẹ, 5: Hàng nặng
    public string RequiredNote { get; set; } = "KHONGCHOXEMHANG"; // CHOTHUHANG, CHOXEMHANGKHONGTHU, KHONGCHOXEMHANG
    public string? Note { get; set; }
    public string? Content { get; set; } = "Test order";

    // Package details
    public int Weight { get; set; } // grams
    public int Length { get; set; } // cm
    public int Width { get; set; } // cm
    public int Height { get; set; } // cm

    // COD
    public int? CodAmount { get; set; }
    public int? CodFailedAmount { get; set; }

    // Insurance
    public int? InsuranceValue { get; set; }

    // Items (required for service_type_id = 5)
    public List<ShippingOrderItem>? Items { get; set; }

    // Optional
    public string? Coupon { get; set; }
    public int? PickStationId { get; set; }
    public int? DeliverStationId { get; set; }
    public List<int>? PickShift { get; set; }
    public long? PickupTime { get; set; } // Unix timestamp

    // Return address
    public string? ReturnPhone { get; set; }
    public string? ReturnAddress { get; set; }
    public int? ReturnDistrictId { get; set; }
    public string? ReturnWardCode { get; set; }
}

public class ShippingOrderItem
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Weight { get; set; }
    public ShippingOrderItemCategory? Category { get; set; }
}

public class ShippingOrderItemCategory
{
    public string? Level1 { get; set; }
    public string? Level2 { get; set; }
    public string? Level3 { get; set; }
}


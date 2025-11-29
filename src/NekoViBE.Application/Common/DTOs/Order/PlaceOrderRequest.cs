using System.Collections.Generic;

namespace NekoViBE.Application.Common.DTOs.Order;

public class PlaceOrderRequest
{
    //Nếu ProductId null nghĩa là đặt hàng thông qua cart, nếu ko null tức là mua ngay 1 sản phẩm
    public Guid? ProductId { get; set; }
    //Nếu Quantity null nghĩa là đặt hàng thông qua cart, nếu ko null tức là mua ngay 1 sản phẩm
    public int? Quantity { get; set; }
    public List<Guid>? UserCouponIds { get; set; }
    public Guid PaymentMethodId { get; set; }
    public Guid? ShippingMethodId { get; set; }
    public Guid? UserAddressId { get; set; } // Địa chỉ giao hàng cho user đã đăng nhập
    public decimal? ShippingAmount { get; set; } // Phí vận chuyển (client tính và truyền lên)

    //Nếu IsOneClickToBuy true nghĩa là đặt hàng thông qua 1 click, cần có email để đặt hàng
    public bool IsOneClick { get; set; } = false;
    public string? GuestEmail { get; set; }
    public string? GuestFirstName { get; set; }
    public string? GuestLastName { get; set; }
    public string? GuestPhone { get; set; }
    public string? OneClickAddress { get; set; }
}   
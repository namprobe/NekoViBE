namespace NekoViBE.Application.Common.Helpers;

/// <summary>
/// Helper class for mapping shipping provider status codes
/// </summary>
public static class ShippingStatusHelper
{
    /// <summary>
    /// Map GHN status string to internal status code and Vietnamese description
    /// Reference: https://api.ghn.vn/home/docs/detail?id=84
    /// </summary>
    public static (int StatusCode, string StatusName, string StatusDescription) MapGHNStatus(string ghnStatus)
    {
        return ghnStatus switch
        {
            "ready_to_pick" => (1, "ready_to_pick", "Chờ lấy hàng"),
            "picking" => (2, "picking", "Đang lấy hàng"),
            "cancel" => (3, "cancel", "Đã hủy"),
            "money_collect_picking" => (4, "money_collect_picking", "Đang thu tiền người gửi"),
            "picked" => (5, "picked", "Đã lấy hàng"),
            "storing" => (6, "storing", "Đang lưu kho"),
            "transporting" => (7, "transporting", "Đang vận chuyển"),
            "sorting" => (8, "sorting", "Đang phân loại"),
            "delivering" => (9, "delivering", "Đang giao hàng"),
            "money_collect_delivering" => (10, "money_collect_delivering", "Đang thu tiền người nhận"),
            "delivered" => (11, "delivered", "Đã giao hàng"),
            "delivery_fail" => (12, "delivery_fail", "Giao hàng thất bại"),
            "waiting_to_return" => (13, "waiting_to_return", "Chờ trả hàng"),
            "return" => (14, "return", "Đang trả hàng"),
            "return_transporting" => (15, "return_transporting", "Đang vận chuyển trả hàng"),
            "return_sorting" => (16, "return_sorting", "Đang phân loại trả hàng"),
            "returning" => (17, "returning", "Đang trả hàng"),
            "return_fail" => (18, "return_fail", "Trả hàng thất bại"),
            "returned" => (19, "returned", "Đã trả hàng"),
            "exception" => (20, "exception", "Ngoại lệ"),
            "damage" => (21, "damage", "Hàng hỏng"),
            "lost" => (22, "lost", "Hàng mất"),
            _ => (0, "unknown", "Không xác định")
        };
    }
}


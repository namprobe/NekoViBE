// using NekoViBE.Domain.Common;

// namespace NekoViBE.Domain.Entities;

// /// <summary>
// /// Phường/Xã - lấy từ GHN API
// /// </summary>
// public class Ward : BaseEntity
// {
//     public string WardCode { get; set; } = string.Empty; // GHN WardCode (string)
//     public int DistrictId { get; set; } // GHN DistrictID (foreign key to District.DistrictId)
//     public string WardName { get; set; } = string.Empty;
//     public string? NameExtension { get; set; } // JSON array of alternative names
//     public bool CanUpdateCod { get; set; } = false;
//     public int SupportType { get; set; } // 0: Khóa tuyến, 1: Lấy/Trả, 2: Giao, 3: Lấy/Giao/Trả
//     public int GHNStatus { get; set; } = 1; // 1: Mở tuyến, 2: Khóa tuyến (from GHN API)
    
//     // navigation properties
//     public virtual District District { get; set; } = null!;
//     public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
// }


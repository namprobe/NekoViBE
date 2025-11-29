// using NekoViBE.Domain.Common;

// namespace NekoViBE.Domain.Entities;

// /// <summary>
// /// Quận/Huyện - lấy từ GHN API
// /// </summary>
// public class District : BaseEntity
// {
//     public int DistrictId { get; set; } // GHN DistrictID
//     public int ProvinceId { get; set; } // GHN ProvinceID (foreign key to Province.ProvinceId)
//     public string DistrictName { get; set; } = string.Empty;
//     public int Code { get; set; } // GHN Code
//     public int Type { get; set; }
//     public int SupportType { get; set; } // 0: Khóa tuyến, 1: Lấy/Trả, 2: Giao, 3: Lấy/Giao/Trả
//     public string? NameExtension { get; set; } // JSON array of alternative names
//     public bool IsEnable { get; set; } = true;
//     public bool CanUpdateCod { get; set; } = false;
//     public int GHNStatus { get; set; } = 1; // 1: Mở tuyến, 2: Khóa tuyến (from GHN API)
    
//     // navigation properties
//     public virtual Province Province { get; set; } = null!;
//     public virtual ICollection<Ward> Wards { get; set; } = new List<Ward>();
//     public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
// }


// using NekoViBE.Domain.Common;

// namespace NekoViBE.Domain.Entities;

// /// <summary>
// /// Tỉnh/Thành phố - lấy từ GHN API
// /// </summary>
// public class Province : BaseEntity
// {
//     public int ProvinceId { get; set; } // GHN ProvinceID
//     public string ProvinceName { get; set; } = string.Empty;
//     public int CountryId { get; set; } = 1; // Default: Vietnam
//     public int Code { get; set; } // GHN Code
//     public string? NameExtension { get; set; } // JSON array of alternative names
//     public bool IsEnable { get; set; } = true;
//     public int RegionId { get; set; }
//     public bool CanUpdateCod { get; set; } = false;
//     public int GHNStatus { get; set; } = 1; // 1: Mở tuyến, 2: Khóa tuyến (from GHN API)
    
//     // navigation properties
//     public virtual ICollection<District> Districts { get; set; } = new List<District>();
//     public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
// }


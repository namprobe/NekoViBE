using Microsoft.EntityFrameworkCore;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Interfaces;

public interface INekoViDbContext
{
    DbSet<CustomerProfile> CustomerProfiles { get; set; }
    DbSet<StaffProfile> StaffProfiles { get; set; }
    DbSet<UserAddress> UserAddresses { get; set; }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using NekoViBE.Infrastructure.Context;
using NekoViBE.Infrastructure.Services;
using System.Text.Json;

namespace NekoViBE.API.Extensions;

public static class SeedingExtension
{
    public static async Task SeedInitialDataAsync(this IApplicationBuilder app, ILogger logger)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var enableSeeding = configuration.GetSection("DataSeeding").GetValue<bool>("EnableSeeding");
        if (!enableSeeding)
        {
            logger.LogInformation("Data seeding is disabled.");
            return;
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // Seed roles from RoleEnum
        foreach (var roleName in Enum.GetNames(typeof(RoleEnum)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new AppRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Status = EntityStatusEnum.Active,
                    CreatedAt = DateTime.UtcNow
                };
                var roleResult = await roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create role {Role}: {Errors}", roleName, errors);
                }
                else
                {
                    logger.LogInformation("Created role: {Role}", roleName);
                }
            }
        }

        // Seed admin user
        var adminEmail = configuration.GetSection("AdminUser").GetValue<string>("Email")?.Trim();
        var adminPassword = configuration.GetSection("AdminUser").GetValue<string>("DefaultPassword");
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("AdminUser configuration is missing. Skipping admin seeding.");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                JoiningAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = EntityStatusEnum.Active
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to create admin user: {Errors}", errors);
                return;
            }
            existingAdmin = admin;
            logger.LogInformation("Created admin user {Email}", adminEmail);
        }

        // Ensure admin is in Admin role
        var adminRoleName = RoleEnum.Admin.ToString();

        if (!await userManager.IsInRoleAsync(existingAdmin, adminRoleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(existingAdmin, adminRoleName);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to add admin user to role {Role}: {Errors}", adminRoleName, errors);
            }
            else
            {
                logger.LogInformation("Added admin user to role {Role}", adminRoleName);
            }
        }

        // Final verification
        var roles = await userManager.GetRolesAsync(existingAdmin);
        logger.LogInformation("Admin user {Email} roles: {Roles}", existingAdmin.Email, string.Join(", ", roles));

        // Seed Payment Methods
        var dbContext = scope.ServiceProvider.GetRequiredService<NekoViDbContext>();
        await SeedPaymentMethodsAsync(dbContext, logger);

        // Seed Shipping Methods
        await SeedShippingMethodsAsync(dbContext, logger);

        // Seed Address Data from GHN API (TP.HCM only for now)
        //await SeedAddressDataAsync(dbContext, scope, logger);

        logger.LogInformation("Data seeding completed.");
    }

    private static async Task SeedPaymentMethodsAsync(NekoViDbContext dbContext, ILogger logger)
    {
        foreach (var gatewayType in Enum.GetValues<PaymentGatewayType>())
        {
            var processorName = gatewayType.ToString().ToLower();
            var name = gatewayType.ToString();
            
            // Check if payment method already exists
            var existing = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.ProcessorName == processorName);

            if (existing == null)
            {
                // Determine if should be active (VnPay and Momo)
                var isActive = gatewayType == PaymentGatewayType.VnPay || gatewayType == PaymentGatewayType.Momo;
                
                var paymentMethod = new PaymentMethod
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    ProcessorName = processorName,
                    IsOnlinePayment = true,
                    ProcessingFee = 0,
                    Status = isActive ? EntityStatusEnum.Active : EntityStatusEnum.Inactive,
                    CreatedAt = DateTime.UtcNow
                };

                // Add descriptions
                paymentMethod.Description = gatewayType switch
                {
                    PaymentGatewayType.VnPay => "Thanh toán qua cổng VNPay",
                    PaymentGatewayType.Momo => "Thanh toán qua ví MoMo",
                    PaymentGatewayType.PayPal => "Thanh toán qua PayPal",
                    _ => null
                };

                dbContext.Set<PaymentMethod>().Add(paymentMethod);
                logger.LogInformation("Created payment method: {Name} (Processor: {Processor}, Status: {Status})", 
                    name, processorName, isActive ? "Active" : "Inactive");
            }
            else
            {
                logger.LogInformation("Payment method {Name} already exists, skipping.", name);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Payment methods seeding completed.");
    }

    private static async Task SeedShippingMethodsAsync(NekoViDbContext dbContext, ILogger logger)
    {
        foreach (var providerType in Enum.GetValues<ShippingProviderType>())
        {
            var name = providerType.ToString();
            
            // Check if shipping method already exists
            var existing = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(sm => sm.Name == name);

            if (existing == null)
            {
                // Determine if should be active (GHN only)
                var isActive = providerType == ShippingProviderType.GHN;
                
                var shippingMethod = new ShippingMethod
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Cost = 0, // Will be calculated dynamically from GHN API
                    EstimatedDays = null, // Will be calculated dynamically from GHN API
                    Status = isActive ? EntityStatusEnum.Active : EntityStatusEnum.Inactive,
                    CreatedAt = DateTime.UtcNow
                };

                // Add descriptions
                shippingMethod.Description = providerType switch
                {
                    ShippingProviderType.GHN => "Giao hàng nhanh qua GHN",
                    ShippingProviderType.GHTK => "Giao hàng tiết kiệm",
                    ShippingProviderType.ViettelPost => "Bưu điện Viettel",
                    ShippingProviderType.JNT => "J&T Express",
                    _ => null
                };

                dbContext.Set<ShippingMethod>().Add(shippingMethod);
                logger.LogInformation("Created shipping method: {Name} (Status: {Status})", 
                    name, isActive ? "Active" : "Inactive");
            }
            else
            {
                logger.LogInformation("Shipping method {Name} already exists, skipping.", name);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Shipping methods seeding completed.");
    }

//     private static async Task SeedAddressDataAsync(NekoViDbContext dbContext, IServiceScope scope, ILogger logger)
//     {
//         try
//         {
//             logger.LogInformation("Starting address data seeding from GHN API...");

//             var ghnAddressService = scope.ServiceProvider.GetRequiredService<GHNAddressService>();
            
//             // 1. Get provinces (filter for TP.HCM only - ProvinceId = 202)
//             var provinces = await ghnAddressService.GetProvincesAsync();
//             var hcmProvince = provinces.FirstOrDefault(p => p.ProvinceId == 202 || 
//                 p.ProvinceName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) ||
//                 p.NameExtension?.Any(ext => ext.Contains("HCM", StringComparison.OrdinalIgnoreCase) || 
//                                            ext.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase)) == true);

//             if (hcmProvince == null)
//             {
//                 logger.LogWarning("TP.HCM province not found in GHN API response");
//                 return;
//             }

//             logger.LogInformation("Found TP.HCM province: {ProvinceName} (ID: {ProvinceId})", 
//                 hcmProvince.ProvinceName, hcmProvince.ProvinceId);

//             // Check if province already exists
//             var existingProvince = await dbContext.Provinces
//                 .FirstOrDefaultAsync(p => p.ProvinceId == hcmProvince.ProvinceId);

//             Province provinceEntity;
//             if (existingProvince == null)
//             {
//                 provinceEntity = new Province
//                 {
//                     Id = Guid.NewGuid(),
//                     ProvinceId = hcmProvince.ProvinceId,
//                     ProvinceName = hcmProvince.ProvinceName,
//                     CountryId = hcmProvince.CountryId,
//                     Code = hcmProvince.Code,
//                     NameExtension = hcmProvince.NameExtension != null ? JsonSerializer.Serialize(hcmProvince.NameExtension) : null,
//                     IsEnable = hcmProvince.IsEnable == 1,
//                     RegionId = hcmProvince.RegionId,
//                     CanUpdateCod = hcmProvince.CanUpdateCod == "true",
//                     Status = EntityStatusEnum.Active,
//                     CreatedAt = DateTime.UtcNow
//                 };
//                 dbContext.Provinces.Add(provinceEntity);
//                 await dbContext.SaveChangesAsync();
//                 logger.LogInformation("Created province: {ProvinceName}", hcmProvince.ProvinceName);
//             }
//             else
//             {
//                 provinceEntity = existingProvince;
//                 logger.LogInformation("Province {ProvinceName} already exists", hcmProvince.ProvinceName);
//             }

//             // 2. Get districts for TP.HCM
//             var districts = await ghnAddressService.GetDistrictsAsync(hcmProvince.ProvinceId);
//             logger.LogInformation("Found {Count} districts for TP.HCM", districts.Count);

//             var districtGuidMap = new Dictionary<int, Guid>(); // Map DistrictId -> Guid

//             foreach (var districtData in districts)
//             {
//                 var existingDistrict = await dbContext.Districts
//                     .FirstOrDefaultAsync(d => d.DistrictId == districtData.DistrictId);

//                 District districtEntity;
//                 if (existingDistrict == null)
//                 {
//                     districtEntity = new District
//                     {
//                         Id = Guid.NewGuid(),
//                         DistrictId = districtData.DistrictId,
//                         ProvinceId = districtData.ProvinceId,
//                         DistrictName = districtData.DistrictName,
//                         Code = districtData.Code,
//                         Type = districtData.Type,
//                         SupportType = districtData.SupportType,
//                         NameExtension = districtData.NameExtension != null ? JsonSerializer.Serialize(districtData.NameExtension) : null,
//                         IsEnable = districtData.IsEnable == 1,
//                         CanUpdateCod = districtData.CanUpdateCod == "true",
//                         GHNStatus = districtData.Status,
//                         Status = EntityStatusEnum.Active,
//                         CreatedAt = DateTime.UtcNow
//                     };
//                     dbContext.Districts.Add(districtEntity);
//                     await dbContext.SaveChangesAsync();
//                     logger.LogInformation("Created district: {DistrictName} (ID: {DistrictId})", 
//                         districtData.DistrictName, districtData.DistrictId);
//                 }
//                 else
//                 {
//                     districtEntity = existingDistrict;
//                     logger.LogInformation("District {DistrictName} already exists", districtData.DistrictName);
//                 }

//                 districtGuidMap[districtData.DistrictId] = districtEntity.Id;

//                 // 3. Get wards for each district
//                 var wards = await ghnAddressService.GetWardsAsync(districtData.DistrictId);
//                 logger.LogInformation("Found {Count} wards for district {DistrictName}", 
//                     wards.Count, districtData.DistrictName);

//                 foreach (var wardData in wards)
//                 {
//                     var existingWard = await dbContext.Wards
//                         .FirstOrDefaultAsync(w => w.WardCode == wardData.WardCode);

//                     if (existingWard == null)
//                     {
//                         var wardEntity = new Ward
//                         {
//                             Id = Guid.NewGuid(),
//                             WardCode = wardData.WardCode,
//                             DistrictId = wardData.DistrictId,
//                             WardName = wardData.WardName,
//                             NameExtension = wardData.NameExtension != null ? JsonSerializer.Serialize(wardData.NameExtension) : null,
//                             CanUpdateCod = wardData.CanUpdateCod == "true",
//                             SupportType = wardData.SupportType,
//                             GHNStatus = wardData.Status,
//                             Status = EntityStatusEnum.Active,
//                             CreatedAt = DateTime.UtcNow
//                         };
//                         dbContext.Wards.Add(wardEntity);
//                     }
//                     else
//                     {
//                         logger.LogInformation("Ward {WardName} already exists", wardData.WardName);
//                     }
//                 }

//                 await dbContext.SaveChangesAsync();
//             }

//             logger.LogInformation("Address data seeding completed successfully. " +
//                 "Provinces: 1, Districts: {DistrictCount}, Wards: {WardCount}",
//                 districts.Count, await dbContext.Wards.CountAsync());
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Error seeding address data from GHN API");
//         }
//     }
}



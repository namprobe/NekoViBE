// File: GetDashboardDataQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Dashboard;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Dashboard.Queries.GetDashboardData
{
    public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, Result<DashboardDataDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDashboardDataQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DashboardDataDto>> Handle(GetDashboardDataQuery request, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var orderRepo = _unitOfWork.Repository<Domain.Entities.Order>();
            var userRepo = _unitOfWork.Repository<AppUser>();
            var productRepo = _unitOfWork.Repository<Domain.Entities.Product>();

            // Helper: Tính % thay đổi (tránh chia cho 0)
            double CalcChange(decimal current, decimal previous) =>
                previous == 0 ? (current > 0 ? 100.0 : 0.0) : Math.Round((double)((current - previous) / previous * 100), 1);

            // === USERS ===
            var totalUsers = await userRepo.CountAsync(u => u.Status == EntityStatusEnum.Active);
            var newUsersThisMonth = await userRepo.CountAsync(u => u.CreatedAt >= thisMonthStart);
            var newUsersLastMonth = await userRepo.CountAsync(u => u.CreatedAt >= lastMonthStart && u.CreatedAt < thisMonthStart);

            // === PRODUCTS ===
            var totalProducts = await productRepo.CountAsync(p => p.Status == EntityStatusEnum.Active && !p.IsDeleted);
            var newProductsThisMonth = await productRepo.CountAsync(p => p.CreatedAt >= thisMonthStart);
            var newProductsLastMonth = await productRepo.CountAsync(p => p.CreatedAt >= lastMonthStart && p.CreatedAt < thisMonthStart);

            // === ORDERS & REVENUE ===
            var totalOrders = await orderRepo.CountAsync();
            var ordersThisMonth = await orderRepo.CountAsync(o => o.CreatedAt >= thisMonthStart);
            var ordersLastMonth = await orderRepo.CountAsync(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < thisMonthStart);

            var revenueThisMonth = await orderRepo.GetQueryable()
                .Where(o => o.CreatedAt >= thisMonthStart
                    && o.OrderStatus == OrderStatusEnum.Delivered) // ← SỬA TẠI ĐÂY
                .SumAsync(o => (decimal?)o.FinalAmount, ct) ?? 0m;

            var revenueLastMonth = await orderRepo.GetQueryable()
                .Where(o => o.CreatedAt >= lastMonthStart
                    && o.CreatedAt < thisMonthStart
                    && o.OrderStatus == OrderStatusEnum.Delivered) // ← SỬA TẠI ĐÂY
                .SumAsync(o => (decimal?)o.FinalAmount, ct) ?? 0m;

            var aovThisMonth = ordersThisMonth > 0 ? revenueThisMonth / ordersThisMonth : 0m;
            var aovLastMonth = ordersLastMonth > 0 ? revenueLastMonth / ordersLastMonth : 0m;

            // === ORDER STATUS COUNTS ===
            var pendingOrders = await orderRepo.CountAsync(o => o.PaymentStatus == PaymentStatusEnum.Pending);
            var pendingThisMonth = await orderRepo.CountAsync(o => o.CreatedAt >= thisMonthStart && o.PaymentStatus == PaymentStatusEnum.Pending);
            var pendingLastMonth = await orderRepo.CountAsync(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < thisMonthStart && o.PaymentStatus == PaymentStatusEnum.Pending);

            var processingOrders = await orderRepo.CountAsync(o => o.OrderStatus == OrderStatusEnum.Processing);
            var processingThisMonth = await orderRepo.CountAsync(o => o.CreatedAt >= thisMonthStart && o.OrderStatus == OrderStatusEnum.Processing);
            var processingLastMonth = await orderRepo.CountAsync(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < thisMonthStart && o.OrderStatus == OrderStatusEnum.Processing);

            var completedOrders = await orderRepo.CountAsync(o => o.OrderStatus == OrderStatusEnum.Delivered);
            var completedThisMonth = await orderRepo.CountAsync(o => o.CreatedAt >= thisMonthStart && o.OrderStatus == OrderStatusEnum.Delivered);
            var completedLastMonth = await orderRepo.CountAsync(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < thisMonthStart && o.OrderStatus == OrderStatusEnum.Delivered);

            var totalRevenue = await orderRepo.GetQueryable()
                .Where(o => o.OrderStatus == OrderStatusEnum.Delivered)
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;

            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            // === RECENT ACTIVITIES (tránh dùng .. và ^ trong LINQ to Entities) ===
            var recentOrders = await orderRepo.GetQueryable()
                .OrderByDescending(o => o.CreatedAt)
                .Take(1)
                .ToListAsync(ct);

            var shippingOrders = await orderRepo.GetQueryable()
                .Where(o => o.OrderStatus == OrderStatusEnum.Shipping)
                .OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt)
                .Take(1)
                .ToListAsync(ct);

            var lowStockProducts = await productRepo.GetQueryable()
                .Where(p => p.StockQuantity <= 10 && p.StockQuantity > 0 &&
                           p.Status == EntityStatusEnum.Active && !p.IsDeleted)
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .Take(1)
                .ToListAsync(ct);

            var newUsers = await userRepo.GetQueryable()
                .OrderByDescending(u => u.CreatedAt)
                .Take(1)
                .ToListAsync(ct);

            // Tạo message sau khi ToListAsync (tránh lỗi EF)
            var activities = new List<RecentActivityDto>();

            activities.AddRange(recentOrders.Select(o => new RecentActivityDto
            {
                Message = $"New order received: #{o.Id.ToString().Substring(0, 8).ToUpper()}",
                Timestamp = o.CreatedAt ?? now,
                Type = "order-new"
            }));

            activities.AddRange(shippingOrders.Select(o => new RecentActivityDto
            {
                Message = $"Order #{o.Id.ToString().Substring(0, 8).ToUpper()} is being shipped",
                Timestamp = o.UpdatedAt ?? o.CreatedAt ?? now,
                Type = "order-shipping"
            }));

            activities.AddRange(lowStockProducts.Select(p => new RecentActivityDto
            {
                Message = $"Product '{p.Name}' low stock ({p.StockQuantity} left)",
                Timestamp = p.UpdatedAt ?? p.CreatedAt ?? now,
                Type = "low-stock"
            }));

            activities.AddRange(newUsers.Select(u => new RecentActivityDto
            {
                Message = $"New user registered: {u.FirstName} {u.LastName}".Trim(),
                Timestamp = u.CreatedAt ?? now,
                Type = "user-new"
            }));

            // === RESULT ===
            var result = new DashboardDataDto
            {
                TotalUsers = totalUsers,
                NewUsersThisMonth = newUsersThisMonth,
                NewUsersChangePercent = CalcChange(newUsersThisMonth, newUsersLastMonth),

                TotalProducts = totalProducts,
                NewProductsThisMonth = newProductsThisMonth,
                NewProductsChangePercent = CalcChange(newProductsThisMonth, newProductsLastMonth),

                TotalOrders = totalOrders,
                OrdersThisMonth = ordersThisMonth,
                OrdersChangePercent = CalcChange(ordersThisMonth, ordersLastMonth),

                PendingOrders = pendingOrders,
                PendingThisMonth = pendingThisMonth,
                PendingChangePercent = CalcChange(pendingThisMonth, pendingLastMonth),

                ProcessingOrders = processingOrders,
                ProcessingThisMonth = processingThisMonth,
                ProcessingChangePercent = CalcChange(processingThisMonth, processingLastMonth),

                CompletedOrders = completedOrders,
                CompletedThisMonth = completedThisMonth,
                CompletedChangePercent = CalcChange(completedThisMonth, completedLastMonth),

                TotalRevenue = totalRevenue,
                RevenueThisMonth = revenueThisMonth,
                RevenueChangePercent = CalcChange(revenueThisMonth, revenueLastMonth),

                AvgOrderValue = avgOrderValue,
                AvgOrderValueThisMonth = aovThisMonth,
                AovChangePercent = CalcChange(aovThisMonth, aovLastMonth),

                RecentActivities = activities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList()
            };

            return Result<DashboardDataDto>.Success(result);
        }
    }
}
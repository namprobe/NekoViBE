// Application/Features/Report/Queries/GetDashboardSummary/GetDashboardSummaryQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Reports;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Report.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardSummaryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardSummaryResponse>> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {

        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart.AddDays(-1);

        var response = new DashboardSummaryResponse();

        // === 1. Total Revenue (FinalAmount của Order có OrderStatus = Delivered = 3) ===
        var completedOrders = _unitOfWork.Repository<Domain.Entities.Order>()
            .GetQueryable()
            .Where(o => o.OrderStatus == OrderStatusEnum.Delivered);

        var currentRevenue = await completedOrders
            .Where(o => o.CreatedAt >= currentMonthStart)
            .SumAsync(o => (decimal?)o.FinalAmount, ct) ?? 0;

        var previousRevenue = await completedOrders
            .Where(o => o.CreatedAt >= previousMonthStart && o.CreatedAt <= previousMonthEnd)
            .SumAsync(o => (decimal?)o.FinalAmount, ct) ?? 0;

        response.Revenue = new RevenueSummary
        {

            CurrentMonth = currentRevenue,
            PreviousMonth = previousRevenue,
            PercentageChange = previousRevenue == 0
                ? currentRevenue > 0 ? 100 : 0
                : Math.Round((double)((currentRevenue - previousRevenue) / previousRevenue) * 100, 1)
        };

        // === 2. Orders Count ===
        var allOrders = _unitOfWork.Repository<Domain.Entities.Order>().GetQueryable();

        var currentOrders = await allOrders
            .CountAsync(o => o.CreatedAt >= currentMonthStart, ct);

        var previousOrders = await allOrders
            .CountAsync(o => o.CreatedAt >= previousMonthStart && o.CreatedAt <= previousMonthEnd, ct);

        response.Orders = new OrderSummary
        {
            CurrentMonth = currentOrders,
            PreviousMonth = previousOrders,
            PercentageChange = previousOrders == 0
                ? currentOrders > 0 ? 100 : 0
                : Math.Round((double)(currentOrders - previousOrders) / previousOrders * 100, 1)
        };

        // === 3. Active Users (tạo trong tháng) ===
        var totalUsers = await _unitOfWork.Repository<AppUser>()
            .GetQueryable()
            .CountAsync(u => u.Status == EntityStatusEnum.Active, ct);

        var currentUsers = await _unitOfWork.Repository<AppUser>()
            .GetQueryable()
            .CountAsync(u => u.CreatedAt >= currentMonthStart && u.Status == EntityStatusEnum.Active, ct);


        var previousUsers = await _unitOfWork.Repository<AppUser>()
            .GetQueryable()
            .CountAsync(u => u.CreatedAt >= previousMonthStart && u.CreatedAt <= previousMonthEnd && u.Status == EntityStatusEnum.Active, ct);

        response.ActiveUsers = new UserSummary
        {
            TotalUser = totalUsers,
            CurrentMonth = currentUsers,
            PreviousMonth = previousUsers,
            PercentageChange = previousUsers == 0
                ? currentUsers > 0 ? 100 : 0
                : Math.Round((double)(currentUsers - previousUsers) / previousUsers * 100, 1)
        };

        // === 4. Recent Activities ===
        var activities = new List<RecentActivityItem>();

        // New order received
        var latestOrder = await allOrders
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (latestOrder != null)
        {
            activities.Add(new RecentActivityItem
            {
                Title = "New order received",
                Description = $"Order #{latestOrder.Id.ToString().Substring(0, 8)}",
                TimeAgo = TimeAgo(latestOrder.CreatedAt),
                Color = "blue"
            });
        }

        // Payment processed (PaymentStatus = Completed = 1)
        var latestPaidOrder = await allOrders
            .Where(o => o.PaymentStatus == PaymentStatusEnum.Completed)
            .OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (latestPaidOrder != null)
        {
            activities.Add(new RecentActivityItem
            {
                Title = "Payment processed",
                Description = $"Order #{latestPaidOrder.Id.ToString().Substring(0, 8)}",
                TimeAgo = TimeAgo(latestPaidOrder.UpdatedAt ?? latestPaidOrder.CreatedAt),
                Color = "green"
            });
        }

        // Inventory updated
        var latestInventory = await _unitOfWork.Repository<Domain.Entities.ProductInventory>()
            .GetQueryable()
            .Include(pi => pi.Product)
            .OrderByDescending(pi => pi.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (latestInventory != null)
        {
            activities.Add(new RecentActivityItem
            {
                Title = "Inventory updated",
                Description = latestInventory.Product?.Name ?? "Product no longer exists",
                TimeAgo = TimeAgo(latestInventory.CreatedAt ?? now),
                Color = "yellow"
            });
        }

        response.RecentActivities = activities.OrderBy(a => a.Title).Take(3).ToList();

        return Result<DashboardSummaryResponse>.Success(response);
    }

    private string TimeAgo(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return "unknown time";

        var date = dateTime.Value;
        var diff = DateTime.UtcNow - date;

        if (diff.TotalMinutes < 1) return "just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} minutes ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
        if (diff.TotalDays < 30) return $"{(int)diff.TotalDays} days ago";
        if (diff.TotalDays < 365) return $"{(int)(diff.TotalDays / 30)} months ago";
        return $"{(int)(diff.TotalDays / 365)} years ago";
    }
}
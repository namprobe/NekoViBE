using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums; // Nhớ import Enum
using System;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class EventQueryBuilder
    {
        public static Expression<Func<Event, bool>> BuildPredicate(this EventFilter filter)
        {
            // Mặc định Predicate là True
            var predicate = PredicateBuilder.True<Event>()
                .CombineAnd(x => !x.IsDeleted); // Exclude deleted events by default

            // 1. Logic lọc Status
            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }
            // Nếu muốn mặc định IsOngoing = true thì Status PHẢI là Active (1)
            else if (filter.IsOngoing == true)
            {
                predicate = predicate.CombineAnd(x => x.Status == EntityStatusEnum.Active);
            }

            // 2. Logic tìm kiếm (Search Text) - Giữ nguyên của bạn
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchPredicate = PredicateBuilder.False<Event>();
                searchPredicate = searchPredicate.CombineOr(x => x.Name.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Description != null && x.Description.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Location != null && x.Location.Contains(filter.Search));
                predicate = predicate.CombineAnd(searchPredicate);
            }

            // 3. Logic lọc chi tiết
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                predicate = predicate.CombineAnd(x => x.Name == filter.Name);
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                predicate = predicate.CombineAnd(x => x.Location == filter.Location);
            }

            // --- 4. LOGIC XỬ LÝ NGÀY THÁNG & ONGOING ---
            if (filter.IsOngoing == true)
            {
                var now = DateTime.Now; // Hoặc DateTime.UtcNow tùy cấu hình server

                // Sự kiện đang diễn ra = Đã bắt đầu (<= Now) VÀ Chưa kết thúc (>= Now)
                predicate = predicate.CombineAnd(x => x.StartDate <= now && x.EndDate >= now);
            }
            else
            {
                // Nếu không chọn IsOngoing thì lọc theo khoảng thời gian user nhập (Logic cũ)
                if (filter.StartDate.HasValue)
                {
                    predicate = predicate.CombineAnd(x => x.StartDate >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    predicate = predicate.CombineAnd(x => x.EndDate <= filter.EndDate.Value);
                }
            }

            return predicate;
        }

        public static Expression<Func<Event, object>> BuildOrderBy(this EventFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
            {
                // Nếu đang xem sự kiện Ongoing, thường ưu tiên cái nào sắp kết thúc hiển thị trước
                if (filter.IsOngoing == true)
                {
                    return x => x.EndDate;
                }
                return x => x.CreatedAt!;
            }

            return filter.SortBy.ToLowerInvariant() switch
            {
                "name" => x => x.Name,
                "startdate" => x => x.StartDate,
                "enddate" => x => x.EndDate,
                "location" => x => x.Location ?? "",
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}
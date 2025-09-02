using System.Linq.Expressions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;

namespace NekoViBE.Application.Common.Extensions;

public static class QueryBuilderExtension
{
 public static Expression<Func<T, bool>> BuildBasePredicate<T>(BasePaginationFilter filter) 
            where T : IEntityLike
        {
            
            var predicate = PredicateBuilder.True<T>();
            
            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }
            
            return predicate;
        }
        
        /// <summary>
        /// Build search predicate cho text fields với null-safe navigation properties
        /// </summary>
        public static Expression<Func<T, bool>> BuildSearchPredicate<T>(
            string? searchTerm, 
            params Expression<Func<T, string>>[] searchProperties) 
            where T : IEntityLike
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || !searchProperties.Any())
                return PredicateBuilder.True<T>();
                
            var searchPredicate = PredicateBuilder.False<T>();
            
            foreach (var property in searchProperties)
            {
                try
                {
                    // Tạo expression để search trong property với null safety
                    var parameter = Expression.Parameter(typeof(T));
                    var propertyVisitor = new ReplaceExpressionVisitor(property.Parameters[0], parameter);
                    var propertyExpression = propertyVisitor.Visit(property.Body);
                    
                    // Kiểm tra null cho navigation properties
                    if (propertyExpression != null)
                    {
                        var nullCheckExpression = BuildNullSafeContainsExpression<T>(propertyExpression, searchTerm, parameter);
                        searchPredicate = searchPredicate.CombineOr<T>(nullCheckExpression);
                    }
                }
                catch
                {
                    // Skip invalid property expressions
                    continue;
                }
            }
            
            return searchPredicate;
        }
        
        /// <summary>
        /// Build null-safe Contains expression cho navigation properties
        /// </summary>
        private static Expression<Func<T, bool>> BuildNullSafeContainsExpression<T>(
            Expression propertyExpression, 
            string searchTerm, 
            ParameterExpression parameter)
        {
            // Tạo null check cho tất cả navigation properties trong chain
            var nullChecks = BuildNullChecksForPropertyChain(propertyExpression, parameter);
            
            // Tạo Contains expression
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsExpression = Expression.Call(
                propertyExpression,
                containsMethod!,
                Expression.Constant(searchTerm, typeof(string))
            );
            
            // Combine null checks với Contains
            Expression finalExpression = containsExpression;
            if (nullChecks.Any())
            {
                var allNullChecks = nullChecks.Aggregate((left, right) => Expression.AndAlso(left, right));
                finalExpression = Expression.AndAlso(allNullChecks, containsExpression);
            }
            
            return Expression.Lambda<Func<T, bool>>(finalExpression, parameter);
        }
        
        /// <summary>
        /// Build null checks cho property chain (e.g., x.Developer.DeveloperName)
        /// </summary>
        private static List<Expression> BuildNullChecksForPropertyChain(Expression propertyExpression, ParameterExpression parameter)
        {
            var nullChecks = new List<Expression>();
            var currentExpression = propertyExpression;
            
            // Traverse up the property chain để tìm navigation properties
            while (currentExpression is MemberExpression memberExpression)
            {
                if (memberExpression.Expression != null && memberExpression.Expression != parameter)
                {
                    // Đây là navigation property, thêm null check
                    var notNullExpression = Expression.NotEqual(memberExpression.Expression, Expression.Constant(null));
                    nullChecks.Insert(0, notNullExpression); // Insert at beginning để maintain order
                }
                currentExpression = memberExpression.Expression;
            }
            
            return nullChecks;
        }
        
        /// <summary>
        /// Build date range predicate
        /// </summary>
        public static Expression<Func<T, bool>> BuildDateRangePredicate<T>(
            DateTime? fromDate, 
            DateTime? toDate, 
            Expression<Func<T, DateTime?>> dateProperty) 
            where T : IEntityLike
        {
            var predicate = PredicateBuilder.True<T>();
            
            if (fromDate.HasValue)
            {
                predicate = predicate.CombineAnd(BuildDateComparison(dateProperty, fromDate.Value, true));
            }
            
            if (toDate.HasValue)
            {
                predicate = predicate.CombineAnd(BuildDateComparison(dateProperty, toDate.Value.AddDays(1), false));
            }
            
            return predicate;
        }

        // Helpers cho Auditable entities
        public static Expression<Func<T, bool>> BuildCreatedDateRange<T>(DateTime? fromDate, DateTime? toDate)
            where T : IEntityLike
        {
            return BuildDateRangePredicate<T>(fromDate, toDate, x => x.CreatedAt);
        }

        public static Expression<Func<T, bool>> BuildUpdatedDateRange<T>(DateTime? fromDate, DateTime? toDate)
            where T : IEntityLike
        {
            return BuildDateRangePredicate<T>(fromDate, toDate, x => x.UpdatedAt);
        }

        public static Expression<Func<T, object>>? BuildOrderByCreatedAt<T>() where T : IEntityLike
        {
            return x => x.CreatedAt!;
        }

        public static Expression<Func<T, object>>? BuildOrderByUpdatedAt<T>() where T : IEntityLike
        {
            return x => x.UpdatedAt!;
        }
        
        private static Expression<Func<T, bool>> BuildDateComparison<T>(
            Expression<Func<T, DateTime?>> dateProperty, 
            DateTime compareValue, 
            bool isGreaterThan) 
            
        {
            var parameter = Expression.Parameter(typeof(T));
            var visitor = new ReplaceExpressionVisitor(dateProperty.Parameters[0], parameter);
            var property = visitor.Visit(dateProperty.Body);
            
            if (property == null)
                throw new ArgumentException("Invalid date property expression");
            
            // Handle nullable DateTime comparison
            var hasValueProperty = Expression.Property(property, "HasValue");
            var valueProperty = Expression.Property(property, "Value");
            
            var comparison = isGreaterThan 
                ? Expression.GreaterThanOrEqual(valueProperty, Expression.Constant(compareValue))
                : Expression.LessThan(valueProperty, Expression.Constant(compareValue));
            
            var condition = Expression.AndAlso(hasValueProperty, comparison);
            
            return Expression.Lambda<Func<T, bool>>(condition, parameter);
        }
        
        /// <summary>
        /// Build order by expression
        /// </summary>
        public static Expression<Func<T, object>>? BuildOrderByExpression<T>(string? sortBy) 
            where T : IEntityLike
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return null;
                
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, sortBy);
            var converted = Expression.Convert(property, typeof(object));
            
            return Expression.Lambda<Func<T, object>>(converted, parameter);
        }
    }
    
    /// <summary>
    /// Generic query builder cho specific entity types
    /// </summary>
    public abstract class BaseQueryBuilder<T, TFilter> 
        where T : IEntityLike
        where TFilter : BasePaginationFilter
    {
        protected virtual Expression<Func<T, bool>> BuildBasePredicate(TFilter filter)
        {
            return QueryBuilderExtension.BuildBasePredicate<T>(filter);
        }
        
        protected virtual Expression<Func<T, bool>> BuildSearchPredicate(TFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Search))
                return PredicateBuilder.True<T>();
                
            return QueryBuilderExtension.BuildSearchPredicate<T>(
                filter.Search, 
                GetSearchProperties().ToArray()
            );
        }
        
        protected abstract IEnumerable<Expression<Func<T, string>>> GetSearchProperties();
        
        protected virtual Expression<Func<T, bool>> BuildCustomPredicate(TFilter filter)
        {
            return PredicateBuilder.True<T>();
        }
        
        public virtual Expression<Func<T, bool>> BuildPredicate(TFilter filter)
        {
            var basePredicate = BuildBasePredicate(filter);
            var searchPredicate = BuildSearchPredicate(filter);
            var customPredicate = BuildCustomPredicate(filter);
            
            return basePredicate
                .CombineAnd(searchPredicate)
                .CombineAnd(customPredicate);
        }
        
        public virtual Expression<Func<T, object>>? BuildOrderBy(TFilter filter)
        {
            return QueryBuilderExtension.BuildOrderByExpression<T>(filter.SortBy);
        }
    }
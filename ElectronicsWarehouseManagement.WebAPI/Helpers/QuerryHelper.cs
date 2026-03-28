using ElectronicsWarehouseManagement.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ElectronicsWarehouseManagement.WebAPI.Helpers
{
    public static class QuerryHelper
    {
        public static IQueryable<T> ApplySearchByName<T>(this IQueryable<T> query, string?search, Expression<Func<T, bool>> predicate)
        {
            if (string.IsNullOrEmpty(search))
            {
                return query;
            }
            return query.Where(predicate);

        }
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sortDirection, Expression<Func<T, object>> keySelector)
        {
            if(sortDirection.ToLower() == "desc")
            {
                return query.OrderByDescending(keySelector);
            }
            return query.OrderBy(keySelector);
        }
        public static IQueryable<T> ApplyPaging<T>(
        this IQueryable<T> query,
        PagingRequest request)
        {
            return query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);
        }
    }
}

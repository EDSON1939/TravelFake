namespace Commerce.Application.Common;

public record PagedResult<T>(IEnumerable<T> Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

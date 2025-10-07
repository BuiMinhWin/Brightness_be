using System.Collections.Generic;

namespace MeasurementAPI.Application.Responses
{
    public record PagedResult<T>(IEnumerable<T> Items, int Page, int PageSize, int TotalCount);
}

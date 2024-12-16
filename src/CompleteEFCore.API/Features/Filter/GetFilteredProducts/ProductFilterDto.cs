using CompleteEFCore.API.Features.Filter.Common.Enums;

namespace CompleteEFCore.API.Features.Filter.GetFilteredProducts;

public record ProductFilterDto(List<FilterField<string>>? ProductNameFilters,
                               List<FilterField<int>>? SupplierFilters,
                               List<FilterField<int>>? CategoryFilters,
                               List<FilterField<RangeFilterDto>>? UnitFilters);

public record FilterField<T>(
    T Value,
    LogicalOperator Operator = LogicalOperator.And);

public record RangeFilterDto(float MinValue, float MaxValue);

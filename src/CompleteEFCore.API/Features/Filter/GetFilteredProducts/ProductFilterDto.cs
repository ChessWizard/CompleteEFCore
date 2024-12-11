namespace CompleteEFCore.API.Features.Filter.GetFilteredProducts;

public record ProductFilterDto(
    string? SearchProductNameKeyword,
    int? SupplierId,
    int? CategoryId,
    int? UnitPriceMinValue,
    int? UnitPriceMaxValue,
    int? Discounted);

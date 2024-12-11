namespace CompleteEFCore.API.Features.Common.Models.Response;

public record NorthwindProductDto(
    int ProductId,
    string ProductName,
    int? SupplierId,
    int? CategoryId,
    string QuantityPerUnit,
    decimal? UnitPrice,
    short? UnitsInStock,
    short? UnitsOnOrder,
    short? ReorderLevel,
    bool Discontinued
);
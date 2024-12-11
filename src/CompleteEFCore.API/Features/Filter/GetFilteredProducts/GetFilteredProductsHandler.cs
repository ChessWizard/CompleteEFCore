using System.Net;
using CompleteEFCore.API.Domain.Entities.Northwind;
using CompleteEFCore.API.Features.Common.Models.Response;
using CompleteEFCore.BuildingBlocks.CQRS.Query;
using CompleteEFCore.BuildingBlocks.Result;
using LinqKit;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CompleteEFCore.API.Features.Filter.GetFilteredProducts;

public record GetFilteredProductsQuery(ProductFilterDto ProductFilterDto) : IQuery<Result<List<NorthwindProductDto>>>;

public class GetFilteredProductsQueryHandler(NorthwindContext context) : IQueryHandler<GetFilteredProductsQuery, Result<List<NorthwindProductDto>>>
{
    public async Task<Result<List<NorthwindProductDto>>> Handle(GetFilteredProductsQuery request, CancellationToken cancellationToken)
    {
        var filterPredicate = GetProductFilters(request.ProductFilterDto);
        var products = await context.Products
                                                .AsExpandable()
                                                .Where(filterPredicate)
                                                .ToListAsync(cancellationToken);
        
        var result = products.Adapt<List<NorthwindProductDto>>();
        return Result<List<NorthwindProductDto>>.Success(result, (int)HttpStatusCode.OK);
    }

    private ExpressionStarter<Product> GetProductFilters(ProductFilterDto productFilterDto)
    {
        var productPredicate = PredicateBuilder.New<Product>(true);

        if (productFilterDto.CategoryId.HasValue)
        {
            productPredicate.And(product => product.CategoryId == productFilterDto.CategoryId);
        }
        
        if(productFilterDto.SupplierId.HasValue)
        {
            productPredicate.And(product => product.SupplierId == productFilterDto.SupplierId);
        }

        if (!string.IsNullOrWhiteSpace(productFilterDto.SearchProductNameKeyword))
        {
            productPredicate.And(product => product.ProductName.Contains(productFilterDto.SearchProductNameKeyword));
        }

        if (productFilterDto.UnitPriceMinValue.HasValue)
        {
            productPredicate.And(product => product.UnitPrice >= productFilterDto.UnitPriceMinValue);
        }

        if (productFilterDto.UnitPriceMaxValue.HasValue)
        {
            productPredicate.And(product => product.UnitPrice <= productFilterDto.UnitPriceMaxValue);
        }

        if (productFilterDto.Discounted.HasValue)
        {
            productPredicate.And(product => product.Discontinued == productFilterDto.Discounted);
        }

        return productPredicate;
    }
}
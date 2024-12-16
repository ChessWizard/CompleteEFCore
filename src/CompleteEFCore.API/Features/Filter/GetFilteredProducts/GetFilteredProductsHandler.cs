using System.Linq.Expressions;
using System.Net;
using CompleteEFCore.API.Domain.Entities.Northwind;
using CompleteEFCore.API.Features.Common.Models.Response;
using CompleteEFCore.API.Features.Filter.Common.Enums;
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
        var filters = CreateProductFilters(request.ProductFilterDto);
        var products = await context.Products
                                                .AsExpandable()
                                                .Where(filters)
                                                .ToListAsync(cancellationToken);
        
        var result = products.Adapt<List<NorthwindProductDto>>();
        return Result<List<NorthwindProductDto>>.Success(result, (int)HttpStatusCode.OK);
    }

    private static ExpressionStarter<Product> CreateProductFilters(ProductFilterDto productFilterDto)
    {
        var productPredicate = PredicateBuilder.New<Product>(true);

        if (productFilterDto.CategoryFilters is not null)
        {
            foreach (var categoryFilter in productFilterDto.CategoryFilters)
            {
                ApplyCondition(productPredicate, 
                    product => product.CategoryId.GetValueOrDefault() == categoryFilter.Value,
                    categoryFilter.Operator);
            } 
        }
        
        if(productFilterDto.SupplierFilters is not null)
        {
            foreach (var supplierFilter in productFilterDto.SupplierFilters)
            {
                ApplyCondition(productPredicate,
                    product => product.SupplierId.GetValueOrDefault() == supplierFilter.Value,
                    supplierFilter.Operator);
            }
        }

        if (productFilterDto.ProductNameFilters is not null)
        {
            foreach (var productNameFilter in productFilterDto.ProductNameFilters)
            {
                ApplyCondition(productPredicate,
                    product => product.ProductName.ToLower()
                        .Contains(productNameFilter.Value.ToLower()),
                    productNameFilter.Operator);
            }
        }

        if (productFilterDto.UnitFilters is not null)
        {
            foreach (var unitFilter in productFilterDto.UnitFilters)
            {
                var minValue = unitFilter.Value.MinValue;
                var maxValue = unitFilter.Value.MaxValue;
                
                ApplyCondition(productPredicate,
                    product => product.UnitPrice.GetValueOrDefault() >= minValue &&
                                       product.UnitPrice.GetValueOrDefault() <= maxValue,
                    unitFilter.Operator);
            }
        }

        return productPredicate;
    }

    private static void ApplyCondition(
        ExpressionStarter<Product> predicate,
        Expression<Func<Product, bool>> condition,
        LogicalOperator logicalOperator)
    {
        switch (logicalOperator)
        {
            case LogicalOperator.And:
                predicate.And(condition);
                break;
            case LogicalOperator.Or:
                predicate.Or(condition);
                break;
            default:
                throw new ArgumentException("Invalid logical operator. Use 'AND' or 'OR'.");
        }
    }
}
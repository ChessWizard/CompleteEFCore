using Carter;
using CompleteEFCore.API.Application.Helpers;
using MediatR;

namespace CompleteEFCore.API.Features.Filter.GetFilteredProducts;

public class GetFilteredProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(pattern: "/Filter/GetFilteredProducts",
                handler: async ([AsParameters]ProductFilterDto filters, ISender sender) =>
                {
                    GetFilteredProductsQuery query = new(filters);
                    var result = await sender.Send(query);
                    return result.FromResult();
                })
            .WithName("GetFilteredProducts")
            .WithTags("Filters")
            .WithSummary("GetFilteredProducts Flow")
            .Produces(StatusCodes.Status200OK);
    }
}
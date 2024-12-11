using Carter;
using CompleteEFCore.API.Application.Enums;
using CompleteEFCore.API.Application.Helpers;
using Mapster;
using MediatR;

namespace CompleteEFCore.API.Features.Versus.AdoVersusEf;

public class AdoVersusEfEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(pattern: "/Versuses/AdoVersusEf",
                handler: async (ISender sender, QueryToolType QueryToolType = QueryToolType.EFCore) =>
                {
                    AdoVersusEfQuery query = new(QueryToolType);
                    var result = await sender.Send(query);
                    return result.FromResult();
                })
            .WithName("AdoVersusEf")
            .WithTags("Versuses")
            .WithSummary("AdoVersusEf Flow")
            .Produces(StatusCodes.Status200OK);
    }
}
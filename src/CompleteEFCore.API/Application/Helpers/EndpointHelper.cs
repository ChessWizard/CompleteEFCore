using CompleteEFCore.BuildingBlocks.Result;

namespace CompleteEFCore.API.Application.Helpers;

public static class EndpointHelper
{
    public static IResult FromResult<T>(this BaseResult<T> result)
        => Results.Json(result, statusCode: result.HttpStatusCode);
}
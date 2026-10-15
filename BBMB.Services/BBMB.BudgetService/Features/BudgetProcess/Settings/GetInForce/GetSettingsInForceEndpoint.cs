using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetInForce
{
    public sealed class GetSettingsInForceEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-settings", async (IMediator mediator, CancellationToken ct, [FromQuery] DateTimeOffset? at = null) =>
                    BudgetResults.Ok(await mediator.Send(new GetSettingsInForceQuery(at), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetSettingsInForce")
                .WithTags("BudgetSettings")
                .Produces<Response<IReadOnlyList<SettingInForceDto>>>(StatusCodes.Status200OK);
        }
    }
}

using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList
{
    public sealed class GetCorrectionFlagsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/correction-flags",
                    async (long? cycleId, bool? open, IMediator mediator, CancellationToken ct) =>
                        Results.Ok(await mediator.Send(new GetCorrectionFlagsQuery(cycleId, open ?? true), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetCorrectionFlags")
                .WithTags("CorrectionFlags")
                .Produces<List<CorrectionFlagDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status403Forbidden);
        }
    }
}

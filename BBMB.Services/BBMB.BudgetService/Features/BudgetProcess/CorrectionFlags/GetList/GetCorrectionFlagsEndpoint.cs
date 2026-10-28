using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList
{
    public sealed class GetCorrectionFlagsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: cycleId and open travel in the body, not the URL (security requirement).
            app.MapPost("/api/correction-flags/list",
                    async (CorrectionFlagsListRequestDto? dto, IMediator mediator, CancellationToken ct) =>
                        Results.Ok(await mediator.Send(new GetCorrectionFlagsQuery(dto?.CycleId, dto?.Open ?? true), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetCorrectionFlags")
                .WithTags("CorrectionFlags")
                .Produces<List<CorrectionFlagDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}

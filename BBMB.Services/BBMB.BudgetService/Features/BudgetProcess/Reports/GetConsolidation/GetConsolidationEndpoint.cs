using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation
{
    public sealed class GetConsolidationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: cycleId and the filters travel in the body, not the URL (security requirement).
            app.MapPost("/api/budget-cycles/consolidation",
                    async (ConsolidationRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetConsolidationQuery(dto.CycleId, dto.Category, dto.Wing, dto.GroupBy), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetConsolidation")
                .WithTags("BudgetReports")
                .Produces<Response<ConsolidationResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}

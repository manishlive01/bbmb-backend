using BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory
{
    public sealed class FinaliseCategoryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // F10.Act / F13.Act by category: Budget.View here, the key in the handler and the service (design §8.1).
            app.MapPost("/api/budget-cycles/{cycleId:long}/categories/{category}/finalise",
                    async (long cycleId, string category, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new FinaliseCategoryCommand(cycleId, category), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("FinaliseCategory")
                .WithTags("BudgetProposals")
                .Produces<Response<FinaliseResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<FinaliseResponseDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}

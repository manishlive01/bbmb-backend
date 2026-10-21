using BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit
{
    public sealed class CreateDelegationLimitEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-settings/delegation-limits",
                    async (CreateDelegationLimitDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new CreateDelegationLimitCommand(dto), ct),
                            r => Results.Created("/api/budget-settings/delegation-limits", r)))
                .RequireAuthorization("BudgetRight:Settings.Manage")
                .WithName("CreateDelegationLimit")
                .WithTags("BudgetSettings")
                .Produces<Response<long>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}

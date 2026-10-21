using BBMB.BudgetService.Features.BudgetProcess.Settings.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Create
{
    public sealed class CreateBudgetSettingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-settings", async (CreateBudgetSettingDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.From(await mediator.Send(new CreateBudgetSettingCommand(dto), ct),
                        r => Results.Created("/api/budget-settings", r)))
                .RequireAuthorization("BudgetRight:Settings.Manage")
                .WithName("CreateBudgetSetting")
                .WithTags("BudgetSettings")
                .Produces<Response<long>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}

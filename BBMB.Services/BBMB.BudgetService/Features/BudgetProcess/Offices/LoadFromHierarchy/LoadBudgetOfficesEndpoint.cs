using BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy
{
    public sealed class LoadBudgetOfficesEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-offices/load-from-hierarchy", async (
                    [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] LoadBudgetOfficesDto? dto,
                    IMediator mediator,
                    CancellationToken ct) =>
                {
                    var response = await mediator.Send(new LoadBudgetOfficesCommand(dto ?? new LoadBudgetOfficesDto()), ct);
                    return BudgetResults.Ok(response);
                })
                .RequireAuthorization("BudgetRight:Offices.Load")
                .WithName("LoadBudgetOffices")
                .WithTags("BudgetOffices")
                .Produces<Response<LoadBudgetOfficesResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}

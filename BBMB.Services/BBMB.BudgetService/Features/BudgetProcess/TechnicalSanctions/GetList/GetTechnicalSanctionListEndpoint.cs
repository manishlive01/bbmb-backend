using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetList
{
    public sealed class GetTechnicalSanctionListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/technical-sanctions",
                    async (IMediator mediator, CancellationToken ct, [FromQuery] int? workId = null) =>
                        BudgetResults.Ok(await mediator.Send(new GetTechnicalSanctionListQuery(workId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetTechnicalSanctions")
                .WithTags("TechnicalSanctions")
                .Produces<Response<List<TechnicalSanctionDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}

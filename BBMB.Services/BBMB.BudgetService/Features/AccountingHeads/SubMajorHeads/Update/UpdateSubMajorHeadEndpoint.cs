using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public class UpdateSubMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/accounting-heads/sub-major/{code}", async (string code, UpdateSubMajorHeadDto dto, IMediator mediator) =>
            {
                var command = new UpdateSubMajorHeadCommand(code, dto);
                var response = await mediator.Send(command);
                
                return response.IsSucced 
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("UpdateSubMajorHead")
            .WithTags("SubMajorHeads")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}

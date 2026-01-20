using BBMB.BudgetService.Features.Offices.RemittanceOffices.Create.Dtos;
using BBMB.BudgetService.Features.Pools.Create;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.Create
{
    public class CreateRemittanceEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/remittance", async (CreateRemittanceDto dto, IMediator mediator) =>
            {
                var command = new CreateRemittanceCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.BadRequest(response);
                }

                return Results.Created($"{dto.RoCode}", response);
            })
            .WithName("CreateRemittance")
            .WithTags("OfficeRemitance")
            .WithSummary("Create a new Remittance")
            .WithDescription("Creates a new Remittance entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}

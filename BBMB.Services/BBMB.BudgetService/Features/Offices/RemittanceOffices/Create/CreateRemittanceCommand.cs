using BBMB.BudgetService.Features.Offices.RemittanceOffices.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.Create
{
    public sealed record CreateRemittanceCommand(CreateRemittanceDto dto) : IRequest<Response>;
}

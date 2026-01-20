using BBMB.BudgetService.Features.Offices.RemittanceOffices.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetById
{
    public sealed record GetRemittanceByIdQuery(string Code) : IRequest<GetRemittanceByIdDto>;
}

using BBMB.BudgetService.Features.Offices.AccountOffices.Create.Dtos;
using BBMB.BudgetService.Features.Pools.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.Create
{
    public sealed record CreateAccountOfficeCommand(CreateAccountOfficeDto dto) : IRequest<Response>;
   
}

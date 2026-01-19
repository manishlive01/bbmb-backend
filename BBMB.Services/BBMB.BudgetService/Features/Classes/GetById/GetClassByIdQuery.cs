using BBMB.BudgetService.Features.Classes.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Classes.GetById
{
    public sealed record GetClassByIdQuery(string Code) : IRequest<GetClassByIdResponseDto>;
}

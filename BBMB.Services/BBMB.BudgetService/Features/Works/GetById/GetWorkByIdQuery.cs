using BBMB.BudgetService.Features.Works.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GetById
{
    public sealed record GetWorkByIdQuery(int Id) : IRequest<GetWorkByIdResponseDto>;
}

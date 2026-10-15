using BBMB.BudgetService.Features.Works.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GetByCode
{
    // P4 (design §4.6, §8.6 "GET /api/works/by-code/{workCode}"): look a work up by its string work code,
    // either form — a generated DDD-YYZZ-SSS code or a seven-digit legacy code. Returns the same shape as
    // GET /api/works/{id:int}.
    public sealed record GetWorkByCodeQuery(string WorkCode) : IRequest<GetWorkByIdResponseDto>;
}

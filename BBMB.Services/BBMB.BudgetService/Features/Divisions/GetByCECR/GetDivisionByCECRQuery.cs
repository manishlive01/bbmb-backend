using BBMB.BudgetService.Features.Divisions.GetByCECR.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Divisions.GetByCECR
{
    public sealed record GetDivisionByCECRQuery(
        int CECode,
        int CRCode
    ) : IRequest<List<GetDivisionByCECRResponseDto>>;
}
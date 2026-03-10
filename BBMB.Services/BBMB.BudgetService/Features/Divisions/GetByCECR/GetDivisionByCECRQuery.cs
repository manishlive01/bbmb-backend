using BBMB.BudgetService.Features.Divisions.GetByCECR.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Divisions.GetByCECR
{
    public sealed record GetDivisionByCECRQuery(
        string CECode,
        string CRCode
    ) : IRequest<List<GetDivisionByCECRResponseDto>>;
}
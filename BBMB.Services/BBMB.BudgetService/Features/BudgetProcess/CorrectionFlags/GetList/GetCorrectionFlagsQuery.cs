using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList
{
    public sealed record GetCorrectionFlagsQuery(long? CycleId, bool Open) : IRequest<List<CorrectionFlagDto>>;
}

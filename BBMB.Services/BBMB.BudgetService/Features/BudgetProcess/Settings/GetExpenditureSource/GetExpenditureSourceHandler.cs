using BBMB.BudgetService.Features.BudgetProcess.Settings.GetExpenditureSource.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetExpenditureSource
{
    /// <summary>
    /// Design §8.2: the mode in force for the office and year (<see cref="ExpenditureSourceReads"/>): the latest
    /// T-18 row at or before now, or KEYED where none exists (BUD-025). Actor required (M-20).
    /// </summary>
    public sealed class GetExpenditureSourceHandler(BudgetContext context, IBudgetActorSource actorSource, TimeProvider? clock = null)
        : IRequestHandler<GetExpenditureSourceQuery, Response<ExpenditureSourceDto>>
    {
        public async Task<Response<ExpenditureSourceDto>> Handle(GetExpenditureSourceQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<ExpenditureSourceDto>(BudgetMessages.M20);
            var mode = await ExpenditureSourceReads.ModeInForceAsync(
                context, request.OfficeId, request.FinYear, (clock ?? TimeProvider.System).GetUtcNow(), ct);
            return new Response<ExpenditureSourceDto>
            {
                IsSucced = true,
                Message = "Expenditure source",
                Data = new ExpenditureSourceDto(request.OfficeId, request.FinYear, mode),
            };
        }
    }
}

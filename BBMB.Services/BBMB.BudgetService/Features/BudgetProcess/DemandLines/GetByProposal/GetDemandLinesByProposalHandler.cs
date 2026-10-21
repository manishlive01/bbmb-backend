using System.Text.Json;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal
{
    /// <summary>
    /// Design §8.3 GetByProposal: V-04 rows of a visible proposal (else NOT_FOUND, BUD-031), in serial order;
    /// marked-deleted lines only with includeDeleted (BUD-319); tab AA / AMR (WORK lines by work class),
    /// UNFORESEEN or HEAD; headings per BUD-305.
    /// </summary>
    public sealed class GetDemandLinesByProposalHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetDemandLinesByProposalQuery, Response<GetDemandLinesResponseDto>>
    {
        private sealed record LevelFigureJson(long level_office_id, string figure, decimal? value);

        public async Task<Response<GetDemandLinesResponseDto>> Handle(GetDemandLinesByProposalQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<GetDemandLinesResponseDto>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<GetDemandLinesResponseDto>(BudgetMessages.NotFound);
            var cycle = await context.BudgetCycles.AsNoTracking().Where(c => c.CycleId == p.CycleId)
                .Select(c => new { c.CurrentFinYear, c.NextFinYear }).SingleAsync(ct);

            var q = context.DemandLineDisplay.AsNoTracking().Where(r => r.ProposalId == p.ProposalId);
            if (!request.IncludeDeleted) q = q.Where(r => !r.IsDeleted);
            q = request.Tab switch
            {
                "AA" => q.Where(r => r.LineType == DemandLineType.Work && r.WorkClass == WorkClass.Aa),
                "AMR" => q.Where(r => r.LineType == DemandLineType.Work && r.WorkClass == WorkClass.Amr),
                "UNFORESEEN" => q.Where(r => r.LineType == DemandLineType.Unforeseen),
                "HEAD" => q.Where(r => r.LineType == DemandLineType.Head),
                _ => q,
            };
            var pageNumber = Math.Max(request.PageNumber, 1);
            var pageSize = Math.Min(request.PageSize <= 0 ? 50 : request.PageSize, 500);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderBy(r => r.SerialNo).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var pages = (int)Math.Ceiling(total / (double)pageSize);
            return new Response<GetDemandLinesResponseDto>
            {
                IsSucced = true, Message = "Demand lines retrieved successfully",
                Data = new GetDemandLinesResponseDto
                {
                    ProposalId = p.ProposalId, Category = p.Category,
                    Headings = DemandLineHeadings.For(p.Category, cycle.CurrentFinYear, cycle.NextFinYear),
                    Lines = rows.Select(r => new DemandLineRowDto
                    {
                        DemandLineId = r.DemandLineId, SerialNo = r.SerialNo, LineType = r.LineType, WorkId = r.WorkId,
                        WorkDesc = r.WorkDesc, WorkClass = r.WorkClass, PrecedingAmrWorkId = r.PrecedingAmrWorkId,
                        AccountCodeId = r.AccountCodeId, CompleteCode = r.CompleteCode, HeadDesc = r.HeadDesc,
                        UnforeseenName = r.UnforeseenName, RbeAmount = r.RbeAmount, ObeNextAmount = r.ObeNextAmount,
                        JustificationText = r.JustificationText, OpeningObeCurrent = r.OpeningObeCurrent,
                        OpeningRbePrevious = r.OpeningRbePrevious, ExpenditureToDate = r.ExpenditureToDate,
                        ExpenditurePreviousYear = r.ExpenditurePreviousYear, PctCheckNotApplied = r.PctCheckNotApplied,
                        FloorCheckUnchecked = r.FloorCheckUnchecked, AddedOnReturn = r.AddedOnReturn, IsDeleted = r.IsDeleted,
                        LevelFigures = (JsonSerializer.Deserialize<List<LevelFigureJson>>(r.LevelFigures) ?? [])
                            .Select(f => new LevelFigureDto(f.level_office_id, f.figure, f.value)).ToList(),
                    }).ToList(),
                    Pagination = new PaginationMetadata
                    {
                        TotalCount = total, PageNumber = pageNumber, PageSize = pageSize, TotalPages = pages,
                        HasPreviousPage = pageNumber > 1, HasNextPage = pageNumber < pages,
                    },
                },
            };
        }
    }
}

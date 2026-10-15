using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure
{
    /// <summary>
    /// Design §8.3 KeyOpeningFigure (BUD-306, DB-16): the origin office keys a carried-forward figure only in the
    /// first cycle and only while its proposal has never been submitted (<c>first_submitted_at IS NULL</c>);
    /// otherwise M-19, and DB-16 refuses the write as the backstop. The subject is the line's (WORK: the work;
    /// HEAD: the origin office and head); RBE_PREVIOUS is a Table 4 column, so a WORK line takes OBE_CURRENT
    /// only (M-22); an UNFORESEEN line has none (M-19). One row per (cycle, subject, kind) (T-36 UK): keying
    /// again replaces the amount, audited by <c>fn_audit_row</c>.
    /// </summary>
    public sealed class KeyOpeningFigureHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        TimeProvider? clock = null)
        : IRequestHandler<KeyOpeningFigureCommand, Response>
    {
        public const string ActionCode = "OPENING_FIGURE_KEY";

        public async Task<Response> Handle(KeyOpeningFigureCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var target = await DemandLineWrites.ResolveAsync(
                context, actorSource, officeScope, authorization, httpContextAccessor, request.ProposalId, now, ct);
            if (target.Refusal is { } refusal)
                return refusal;
            var (actor, p) = (target.Actor!, target.Proposal!);
            var isFirstCycle = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == p.CycleId).Select(c => c.IsFirstCycle).SingleAsync(ct);
            if (!isFirstCycle || p.FirstSubmittedAt is not null)
                return BudgetResponses.Fail(BudgetMessages.M19);
            var (line, lineRefusal) = await DemandLineWrites.FindLiveLineAsync(context, p, request.DemandLineId, false, ct);
            if (lineRefusal is not null)
                return lineRefusal;
            if (line!.LineType == DemandLineType.Unforeseen)
                return BudgetResponses.Fail(BudgetMessages.M19);
            var isWork = line.LineType == DemandLineType.Work;
            if (isWork && request.Dto.FigureKind == OpeningFigureKind.RbePrevious)
                return DemandLineWrites.FieldFailure(line, "figureKind", BudgetMessages.M22);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            await DemandLineWrites.LockProposalAsync(context, p.ProposalId, ct);
            var row = await context.OpeningFigures.FirstOrDefaultAsync(o =>
                o.CycleId == p.CycleId && o.FigureKind == request.Dto.FigureKind &&
                (isWork
                    ? o.SubjectType == "WORK" && o.WorkId == line.WorkId
                    : o.SubjectType == "HEAD" && o.OfficeId == p.OriginOfficeId && o.AccountCodeId == line.AccountCodeId), ct);
            if (row is null)
            {
                row = new OpeningFigure
                {
                    CycleId = p.CycleId, SubjectType = isWork ? "WORK" : "HEAD", FigureKind = request.Dto.FigureKind,
                    WorkId = isWork ? line.WorkId : null,
                    OfficeId = isWork ? null : p.OriginOfficeId,
                    AccountCodeId = isWork ? null : line.AccountCodeId,
                };
                context.OpeningFigures.Add(row);
            }
            row.Amount = request.Dto.Amount;
            DemandLineWrites.RenewLease(p, actor, now, ProposalLease.Length(configuration));
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Opening figure recorded", Data = row.OpeningFigureId };
        }
    }
}

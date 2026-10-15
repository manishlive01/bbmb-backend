using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add
{
    /// <summary>
    /// Design §8.3 Add validator: lineType vs the proposal's category (WORK / UNFORESEEN only for WORKS, HEAD
    /// otherwise; DB-01); exactly one of workId / newWork for WORK; the fields each type carries (T-30 CHECKs);
    /// unforeseenName ≤ 500; amounts whole rupees, negatives allowed (BUD-088); justification ≤ 4000.
    /// The category is read only for a proposal visible to the actor, so a 400 never discloses a proposal of
    /// another office (BUD-031); for an invisible one the handler answers NOT_FOUND.
    /// </summary>
    public sealed class AddDemandLineValidator : AbstractValidator<AddDemandLineCommand>
    {
        private static readonly string[] Types = [DemandLineType.Work, DemandLineType.Head, DemandLineType.Unforeseen];

        public AddDemandLineValidator(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.LineType).Must(t => Types.Contains(t))
                    .WithMessage("lineType must be WORK, HEAD or UNFORESEEN.");
                RuleFor(x => x).MustAsync(async (cmd, ct) =>
                    {
                        var actor = await actorSource.GetAsync(ct);
                        if (actor is null || !Types.Contains(cmd.Dto.LineType))
                            return true;
                        var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, cmd.ProposalId, false, ct);
                        return p is null || (cmd.Dto.LineType == DemandLineType.Head) != (p.Category == ProposalCategory.Works);
                    })
                    .WithName("lineType")
                    .WithMessage("lineType does not belong to the category of the proposal: WORK and UNFORESEEN lines are for WORKS, HEAD lines for the other categories.");
                When(x => x.Dto.LineType == DemandLineType.Work, () =>
                {
                    RuleFor(x => x.Dto).Must(d => (d.WorkId is not null) != (d.NewWork is not null))
                        .WithName("workId").WithMessage("Exactly one of workId and newWork is required for a WORK line.");
                    RuleFor(x => x.Dto.WorkId).GreaterThan(0).When(x => x.Dto.WorkId is not null);
                    RuleFor(x => x.Dto.UnforeseenName).Null().WithMessage("unforeseenName applies to UNFORESEEN lines only.");
                    When(x => x.Dto.NewWork is not null, () =>
                    {
                        RuleFor(x => x.Dto.NewWork!.NameOfWork).NotEmpty().MaximumLength(500).WithName("newWork.nameOfWork");
                        RuleFor(x => x.Dto.NewWork!.AccountCodeId).GreaterThan(0).WithName("newWork.accountCodeId");
                        RuleFor(x => x.Dto.NewWork!.WorkClass).Must(c => c is WorkClass.Aa or WorkClass.Amr)
                            .WithName("newWork.workClass").WithMessage("newWork.workClass must be AA or AMR.");
                    });
                });
                When(x => x.Dto.LineType == DemandLineType.Head, () =>
                {
                    RuleFor(x => x.Dto.AccountCodeId).NotNull().WithMessage("accountCodeId is required for a HEAD line.");
                    RuleFor(x => x.Dto.WorkId).Null().WithMessage("workId applies to WORK lines only.");
                    RuleFor(x => x.Dto.NewWork).Null().WithMessage("newWork applies to WORK lines only.");
                    RuleFor(x => x.Dto.UnforeseenName).Null().WithMessage("unforeseenName applies to UNFORESEEN lines only.");
                    RuleFor(x => x.Dto.PrecedingAmrWorkId).Null().WithMessage("precedingAmrWorkId applies to WORK lines only.");
                });
                When(x => x.Dto.LineType == DemandLineType.Unforeseen, () =>
                {
                    RuleFor(x => x.Dto.UnforeseenName).NotEmpty().WithMessage("unforeseenName is required for an UNFORESEEN line.");
                    RuleFor(x => x.Dto.AccountCodeId).Null().WithMessage("An UNFORESEEN line carries no account code (BUD-277).");
                    RuleFor(x => x.Dto.WorkId).Null().WithMessage("workId applies to WORK lines only.");
                    RuleFor(x => x.Dto.NewWork).Null().WithMessage("newWork applies to WORK lines only.");
                    RuleFor(x => x.Dto.PrecedingAmrWorkId).Null().WithMessage("precedingAmrWorkId applies to WORK lines only.");
                });
                RuleFor(x => x.Dto.AccountCodeId).GreaterThan(0).When(x => x.Dto.AccountCodeId is not null);
                RuleFor(x => x.Dto.PrecedingAmrWorkId).GreaterThan(0).When(x => x.Dto.PrecedingAmrWorkId is not null);
                RuleFor(x => x.Dto.UnforeseenName).MaximumLength(500);
                RuleFor(x => x.Dto.RbeAmount).WholeRupees();
                RuleFor(x => x.Dto.ObeNextAmount).WholeRupees();
                RuleFor(x => x.Dto.JustificationText).MaximumLength(4000);
            });
        }
    }
}

using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetAssignableHeads;
using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetList;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetList;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.GetList;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview;
using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search;
using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetById;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory;
using BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe;
using BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Reads;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetEstimateCodes;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetList;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Reads;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.ReadRequests
{
    // Validators for the Budget reads whose ids moved from the URL into a POST body. They validate the existing
    // MediatR query (ValidationBehavior runs on MediatR requests), and name the body field in the error so the
    // 400 "Validation Error" response points at what the caller sent.

    public sealed class GetHeadAssignmentsQueryValidator : AbstractValidator<GetHeadAssignmentsQuery>
    {
        public GetHeadAssignmentsQueryValidator() =>
            RuleFor(x => x.OfficeId!.Value).GreaterThan(0).When(x => x.OfficeId.HasValue)
                .OverridePropertyName("officeId").WithMessage("officeId must be greater than 0.");
    }

    public sealed class GetAdminApprovalListQueryValidator : AbstractValidator<GetAdminApprovalListQuery>
    {
        public GetAdminApprovalListQueryValidator() =>
            RuleFor(x => x.WorkId!.Value).GreaterThan(0).When(x => x.WorkId.HasValue)
                .OverridePropertyName("workId").WithMessage("workId must be greater than 0.");
    }

    public sealed class GetTechnicalSanctionListQueryValidator : AbstractValidator<GetTechnicalSanctionListQuery>
    {
        public GetTechnicalSanctionListQueryValidator() =>
            RuleFor(x => x.WorkId!.Value).GreaterThan(0).When(x => x.WorkId.HasValue)
                .OverridePropertyName("workId").WithMessage("workId must be greater than 0.");
    }

    public sealed class PreviewAllocationQueryValidator : AbstractValidator<PreviewAllocationQuery>
    {
        public PreviewAllocationQueryValidator()
        {
            RuleFor(x => x.CycleId).GreaterThan(0).OverridePropertyName("cycleId").WithMessage("cycleId must be greater than 0.");
            // The allowed values (BSC / FULL_BOARD) stay the handler's check so its catalogued error code is unchanged.
            RuleFor(x => x.Body).NotEmpty().OverridePropertyName("body").WithMessage("body is required.");
        }
    }

    public sealed class GetConsolidationQueryValidator : AbstractValidator<GetConsolidationQuery>
    {
        public GetConsolidationQueryValidator() =>
            RuleFor(x => x.CycleId).GreaterThan(0).OverridePropertyName("cycleId").WithMessage("cycleId must be greater than 0.");
    }

    public sealed class GetDistributionsQueryValidator : AbstractValidator<GetDistributionsQuery>
    {
        public GetDistributionsQueryValidator()
        {
            RuleFor(x => x.CycleId).GreaterThan(0).OverridePropertyName("cycleId").WithMessage("cycleId must be greater than 0.");
            RuleFor(x => x.Category).NotEmpty().OverridePropertyName("category").WithMessage("category is required.");
            RuleFor(x => x.CeOfficeId).GreaterThan(0).OverridePropertyName("ceOfficeId").WithMessage("ceOfficeId must be greater than 0.");
        }
    }

    public sealed class GetProposalByIdQueryValidator : AbstractValidator<GetProposalByIdQuery>
    {
        public GetProposalByIdQueryValidator() =>
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
    }

    public sealed class GetAmrPredecessorOptionsQueryValidator : AbstractValidator<GetAmrPredecessorOptionsQuery>
    {
        public GetAmrPredecessorOptionsQueryValidator() =>
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
    }

    public sealed class GetAssignableHeadsQueryValidator : AbstractValidator<GetAssignableHeadsQuery>
    {
        public GetAssignableHeadsQueryValidator() =>
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
    }

    public sealed class GetMovementsQueryValidator : AbstractValidator<GetMovementsQuery>
    {
        public GetMovementsQueryValidator() =>
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
    }

    public sealed class GetObservationsByProposalQueryValidator : AbstractValidator<GetObservationsByProposalQuery>
    {
        public GetObservationsByProposalQueryValidator() =>
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
    }

    public sealed class GetDemandLinesByProposalQueryValidator : AbstractValidator<GetDemandLinesByProposalQuery>
    {
        public GetDemandLinesByProposalQueryValidator() =>
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
    }

    public sealed class GetFigureHistoryQueryValidator : AbstractValidator<GetFigureHistoryQuery>
    {
        public GetFigureHistoryQueryValidator()
        {
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
            RuleFor(x => x.DemandLineId).GreaterThan(0).OverridePropertyName("lineId").WithMessage("lineId must be greater than 0.");
        }
    }

    public sealed class GetCorrectionFlagsQueryValidator : AbstractValidator<GetCorrectionFlagsQuery>
    {
        public GetCorrectionFlagsQueryValidator() =>
            RuleFor(x => x.CycleId!.Value).GreaterThan(0).When(x => x.CycleId.HasValue)
                .OverridePropertyName("cycleId").WithMessage("cycleId must be greater than 0.");
    }

    public sealed class GetReappropriationByIdQueryValidator : AbstractValidator<GetReappropriationByIdQuery>
    {
        public GetReappropriationByIdQueryValidator() =>
            RuleFor(x => x.Id).GreaterThan(0).OverridePropertyName("reappropriationId").WithMessage("reappropriationId must be greater than 0.");
    }

    public sealed class GetUnforeseenByIdQueryValidator : AbstractValidator<GetUnforeseenByIdQuery>
    {
        public GetUnforeseenByIdQueryValidator() =>
            RuleFor(x => x.Id).GreaterThan(0).OverridePropertyName("unforeseenRequestId").WithMessage("unforeseenRequestId must be greater than 0.");
    }
    public sealed class GetDemandLineByIdQueryValidator : AbstractValidator<GetDemandLineByIdQuery>
    {
        public GetDemandLineByIdQueryValidator()
        {
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
            RuleFor(x => x.DemandLineId).GreaterThan(0).OverridePropertyName("lineId").WithMessage("lineId must be greater than 0.");
        }
    }
    public sealed class GetReappropriationSourcesQueryValidator : AbstractValidator<GetReappropriationSourcesQuery>
    {
        public GetReappropriationSourcesQueryValidator() =>
            RuleFor(x => x.ReappropriationId).GreaterThan(0).OverridePropertyName("reappropriationId").WithMessage("reappropriationId must be greater than 0.");
    }
    public sealed class GetAllocationsQueryValidator : AbstractValidator<GetAllocationsQuery>
    {
        public GetAllocationsQueryValidator() =>
            RuleFor(x => x.CycleId).GreaterThan(0).OverridePropertyName("cycleId").WithMessage("cycleId must be greater than 0.");
    }
    public sealed class GetUnforeseenAuthorisationByIdQueryValidator : AbstractValidator<GetUnforeseenAuthorisationByIdQuery>
    {
        public GetUnforeseenAuthorisationByIdQueryValidator() =>
            RuleFor(x => x.UnforeseenAuthorisationId).GreaterThan(0)
                .OverridePropertyName("unforeseenAuthorisationId").WithMessage("unforeseenAuthorisationId must be greater than 0.");
    }
    public sealed class GetUnforeseenProvisionsQueryValidator : AbstractValidator<GetUnforeseenProvisionsQuery>
    {
        public GetUnforeseenProvisionsQueryValidator()
        {
            RuleFor(x => x.CycleId!.Value).GreaterThan(0).When(x => x.CycleId.HasValue)
                .OverridePropertyName("cycleId").WithMessage("cycleId must be greater than 0.");
            RuleFor(x => x.OfficeId!.Value).GreaterThan(0).When(x => x.OfficeId.HasValue)
                .OverridePropertyName("officeId").WithMessage("officeId must be greater than 0.");
        }
    }
    public sealed class SearchAuditTrailQueryValidator : AbstractValidator<SearchAuditTrailQuery>
    {
        public SearchAuditTrailQueryValidator()
        {
            RuleFor(x => x.CycleId!.Value).GreaterThan(0).When(x => x.CycleId.HasValue)
                .OverridePropertyName("cycleId").WithMessage("cycleId must be greater than 0.");
            RuleFor(x => x.OfficeId!.Value).GreaterThan(0).When(x => x.OfficeId.HasValue)
                .OverridePropertyName("officeId").WithMessage("officeId must be greater than 0.");
        }
    }
    public sealed class GetEstimateCodesByWorkCodeQueryValidator : AbstractValidator<GetEstimateCodesByWorkCodeQuery>
    {
        // 12 = Work.WorkCode [MaxLength(12)]: DDD-YYZZ-SSS generated, or a seven-digit legacy code (BUD-231).
        public const int WorkCodeMaxLength = 12;
        public GetEstimateCodesByWorkCodeQueryValidator() =>
            RuleFor(x => x.WorkCode).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("workCode is required.")
                .MaximumLength(WorkCodeMaxLength).WithMessage($"workCode must be at most {WorkCodeMaxLength} characters.")
                .OverridePropertyName("workCode");
    }
}

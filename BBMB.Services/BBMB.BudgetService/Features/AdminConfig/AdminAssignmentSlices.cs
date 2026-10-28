using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AdminConfig
{
    // Requirement 4 (design §6): the document-backed admin-assignment lifecycle
    //   create (PENDING, role must be is_admin) -> upload-document -> approve (APPROVED) -> revoke (REVOKED).
    // A role is an EFFECTIVE admin only once APPROVED with a document (IBudgetConfigProvider.AdminRoles reflects
    // this). Rules are enforced in the handler for a friendly message AND by the DB (ck_admin_approved_has_doc,
    // ux_admin_role_active) as the owning invariant. Every write invalidates BudgetConfigCache so the change
    // applies without a restart. POST only, JSON bodies, no ids in the URL. All gated by Settings.Admin.
    //
    // This slice is deliberately placed OUTSIDE Features.BudgetProcess.* so it is excluded from the
    // BudgetEndpointPolicyTests area/S81 assertions, which cover the existing budget areas only (design §6).

    public sealed record AdminAssignmentDto(
        long Id, string RoleName, string Status, string? DocumentRef,
        Guid? UploadedBy, DateTimeOffset? UploadedAt, Guid? ApprovedBy, DateTimeOffset? ApprovedAt,
        Guid? RevokedBy, DateTimeOffset? RevokedAt, bool IsEffectiveAdmin);

    public sealed record CreateAssignmentDto(string RoleName);
    public sealed record UploadAssignmentDocumentDto(string RoleName, string DocumentRef);
    public sealed record ApproveAssignmentDto(string RoleName);
    public sealed record RevokeAssignmentDto(string RoleName, string Reason);

    public sealed record CreateAssignmentCommand(CreateAssignmentDto Dto) : IRequest<Response<long>>;
    public sealed record UploadAssignmentDocumentCommand(UploadAssignmentDocumentDto Dto) : IRequest<Response>;
    public sealed record ApproveAssignmentCommand(ApproveAssignmentDto Dto) : IRequest<Response>;
    public sealed record RevokeAssignmentCommand(RevokeAssignmentDto Dto) : IRequest<Response>;
    public sealed record ListAssignmentsQuery : IRequest<Response<IReadOnlyList<AdminAssignmentDto>>>;

    public sealed class CreateAssignmentValidator : AbstractValidator<CreateAssignmentCommand>
    {
        public CreateAssignmentValidator() => RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
    }

    public sealed class UploadAssignmentDocumentValidator : AbstractValidator<UploadAssignmentDocumentCommand>
    {
        public UploadAssignmentDocumentValidator()
        {
            RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Dto.DocumentRef).NotEmpty().MaximumLength(200);
        }
    }

    public sealed class ApproveAssignmentValidator : AbstractValidator<ApproveAssignmentCommand>
    {
        public ApproveAssignmentValidator() => RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
    }

    public sealed class RevokeAssignmentValidator : AbstractValidator<RevokeAssignmentCommand>
    {
        public RevokeAssignmentValidator()
        {
            RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Dto.Reason).NotEmpty().MaximumLength(500);
        }
    }

    /// <summary>Shared loads for the admin-assignment handlers.</summary>
    public abstract class AdminAssignmentHandlerBase(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
    {
        protected BudgetContext Context { get; } = context;
        protected IBudgetActorSource ActorSource { get; } = actorSource;
        protected IBudgetActionScope Scope { get; } = scope;
        protected IBudgetConfigProvider Config { get; } = config;

        protected Task<BudgetRole?> RoleByNameAsync(string roleName, CancellationToken ct) =>
            Context.BudgetRoles.FirstOrDefaultAsync(r => r.RoleName == roleName, ct);

        /// <summary>The one live (non-revoked) assignment of a role, or null.</summary>
        protected Task<BudgetAdminRoleAssignment?> LiveAssignmentAsync(long roleId, CancellationToken ct) =>
            Context.BudgetAdminRoleAssignments
                .FirstOrDefaultAsync(a => a.RoleId == roleId && a.Status != AdminAssignmentStatus.Revoked, ct);
    }

    public sealed class CreateAssignmentHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : AdminAssignmentHandlerBase(context, actorSource, scope, config),
          IRequestHandler<CreateAssignmentCommand, Response<long>>
    {
        public const string ActionCode = "ADMIN_ASSIGNMENT_CREATE";

        public async Task<Response<long>> Handle(CreateAssignmentCommand request, CancellationToken ct)
        {
            if (await ActorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);

            var role = await RoleByNameAsync(request.Dto.RoleName, ct);
            if (role is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M05);
            // Only a role marked is_admin may be designated a Budget administrator.
            if (!role.IsAdmin)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            // One live assignment per role (also the DB partial UK ux_admin_role_active).
            if (await LiveAssignmentAsync(role.Id, ct) is not null)
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            var row = new BudgetAdminRoleAssignment { RoleId = role.Id, Status = AdminAssignmentStatus.Pending };
            await using var tx = await Scope.BeginAsync(ActionCode, ct: ct);
            Context.BudgetAdminRoleAssignments.Add(row);
            await Context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            Config.Invalidate();
            return new Response<long> { IsSucced = true, Message = "Admin assignment created (PENDING)", Data = row.Id };
        }
    }

    public sealed class UploadAssignmentDocumentHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : AdminAssignmentHandlerBase(context, actorSource, scope, config),
          IRequestHandler<UploadAssignmentDocumentCommand, Response>
    {
        public const string ActionCode = "ADMIN_ASSIGNMENT_UPLOAD_DOCUMENT";

        public async Task<Response> Handle(UploadAssignmentDocumentCommand request, CancellationToken ct)
        {
            var actor = await ActorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var role = await RoleByNameAsync(request.Dto.RoleName, ct);
            if (role is null)
                return BudgetResponses.Fail(BudgetMessages.M05);
            var assignment = await LiveAssignmentAsync(role.Id, ct);
            if (assignment is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            // A document is uploaded before approval; once APPROVED the assignment is closed to document changes.
            if (assignment.Status != AdminAssignmentStatus.Pending)
                return BudgetResponses.Fail(BudgetMessages.M19);

            await using var tx = await Scope.BeginAsync(ActionCode, ct: ct);
            assignment.DocumentRef = request.Dto.DocumentRef.Trim();
            assignment.UploadedBy = actor.EmployeeId;
            assignment.UploadedAt = DateTimeOffset.UtcNow;
            await Context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            Config.Invalidate();
            return new Response { IsSucced = true, Message = "Signed document recorded" };
        }
    }

    public sealed class ApproveAssignmentHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : AdminAssignmentHandlerBase(context, actorSource, scope, config),
          IRequestHandler<ApproveAssignmentCommand, Response>
    {
        public const string ActionCode = "ADMIN_ASSIGNMENT_APPROVE";

        public async Task<Response> Handle(ApproveAssignmentCommand request, CancellationToken ct)
        {
            var actor = await ActorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var role = await RoleByNameAsync(request.Dto.RoleName, ct);
            if (role is null)
                return BudgetResponses.Fail(BudgetMessages.M05);
            var assignment = await LiveAssignmentAsync(role.Id, ct);
            if (assignment is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (assignment.Status != AdminAssignmentStatus.Pending)
                return BudgetResponses.Fail(BudgetMessages.M19);
            // "APPROVED ⇒ has document" (DB owns it via ck_admin_approved_has_doc); re-checked for a message.
            if (string.IsNullOrWhiteSpace(assignment.DocumentRef) || assignment.UploadedBy is null)
                return BudgetResponses.Fail(BudgetMessages.M04);
            // Uploader ≠ approver (segregation of duties).
            if (assignment.UploadedBy == actor.EmployeeId)
                return BudgetResponses.Fail(BudgetMessages.M20);
            // The approver must itself be an effective admin or act at IT_CELL level at approval time.
            var snapshot = await Config.GetAsync(ct);
            var approverIsAdmin = snapshot.AdminRoles.Contains(actor.RoleCode, StringComparer.OrdinalIgnoreCase);
            var approverIsItCell = string.Equals(actor.OfficeLevel, OfficeLevel.ItCell, StringComparison.Ordinal);
            if (!approverIsAdmin && !approverIsItCell)
                return BudgetResponses.Fail(BudgetMessages.M20);

            await using var tx = await Scope.BeginAsync(ActionCode, ct: ct);
            assignment.Status = AdminAssignmentStatus.Approved;
            assignment.ApprovedBy = actor.EmployeeId;
            assignment.ApprovedAt = DateTimeOffset.UtcNow;
            await Context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            Config.Invalidate();
            return new Response { IsSucced = true, Message = "Admin assignment approved" };
        }
    }

    public sealed class RevokeAssignmentHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : AdminAssignmentHandlerBase(context, actorSource, scope, config),
          IRequestHandler<RevokeAssignmentCommand, Response>
    {
        public const string ActionCode = "ADMIN_ASSIGNMENT_REVOKE";

        public async Task<Response> Handle(RevokeAssignmentCommand request, CancellationToken ct)
        {
            var actor = await ActorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var role = await RoleByNameAsync(request.Dto.RoleName, ct);
            if (role is null)
                return BudgetResponses.Fail(BudgetMessages.M05);
            var assignment = await LiveAssignmentAsync(role.Id, ct);
            if (assignment is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);

            await using var tx = await Scope.BeginAsync(ActionCode, ct: ct);
            assignment.Status = AdminAssignmentStatus.Revoked;
            assignment.RevokedBy = actor.EmployeeId;
            assignment.RevokedAt = DateTimeOffset.UtcNow;
            assignment.RevokeReason = request.Dto.Reason.Trim();
            await Context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            Config.Invalidate();
            return new Response { IsSucced = true, Message = "Admin assignment revoked" };
        }
    }

    public sealed class ListAssignmentsHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<ListAssignmentsQuery, Response<IReadOnlyList<AdminAssignmentDto>>>
    {
        public async Task<Response<IReadOnlyList<AdminAssignmentDto>>> Handle(ListAssignmentsQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<AdminAssignmentDto>>(BudgetMessages.M20);

            var rows = await (
                from a in context.BudgetAdminRoleAssignments.AsNoTracking()
                join r in context.BudgetRoles.AsNoTracking() on a.RoleId equals r.Id
                orderby r.RoleName, a.Id
                select new { a, r.RoleName }).ToListAsync(ct);

            var data = rows.Select(x => new AdminAssignmentDto(
                x.a.Id, x.RoleName, x.a.Status, x.a.DocumentRef,
                x.a.UploadedBy, x.a.UploadedAt, x.a.ApprovedBy, x.a.ApprovedAt,
                x.a.RevokedBy, x.a.RevokedAt, x.a.IsEffectiveAdmin())).ToList();

            return new Response<IReadOnlyList<AdminAssignmentDto>>
            {
                IsSucced = true, Message = "Admin assignments", Data = data,
            };
        }
    }

    public sealed class AdminAssignmentEndpoints : IEndpoint
    {
        private const string Policy = "BudgetRight:Settings.Admin";

        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-admin/assignments/create",
                    async (CreateAssignmentDto dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.From(await m.Send(new CreateAssignmentCommand(dto), ct),
                            r => Results.Created("/api/budget-admin/assignments", r)))
                .RequireAuthorization(Policy).WithName("CreateAdminAssignment").WithTags("BudgetAdmin")
                .Produces<Response<long>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-admin/assignments/upload-document",
                    async (UploadAssignmentDocumentDto dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new UploadAssignmentDocumentCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("UploadAdminAssignmentDocument").WithTags("BudgetAdmin")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-admin/assignments/approve",
                    async (ApproveAssignmentDto dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new ApproveAssignmentCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("ApproveAdminAssignment").WithTags("BudgetAdmin")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-admin/assignments/revoke",
                    async (RevokeAssignmentDto dto, IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new RevokeAssignmentCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("RevokeAdminAssignment").WithTags("BudgetAdmin")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-admin/assignments/list",
                    async (IMediator m, CancellationToken ct) =>
                        BudgetResults.Ok(await m.Send(new ListAssignmentsQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListAdminAssignments").WithTags("BudgetAdmin")
                .Produces<Response<IReadOnlyList<AdminAssignmentDto>>>(StatusCodes.Status200OK);
        }
    }
}

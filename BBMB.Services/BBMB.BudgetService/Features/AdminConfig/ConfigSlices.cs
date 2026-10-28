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
    // Requirement 1 (design §6): list/upsert the authz-config so an authorized change takes effect without a
    // config edit or redeploy (within the cache TTL; every upsert invalidates BudgetConfigCache). POST only,
    // JSON bodies, no ids in the URL. All gated by Settings.Admin. Namespace is outside Features.BudgetProcess.*
    // so BudgetEndpointPolicyTests' area/S81 assertions do not apply (design §6).

    // -------------------- settings --------------------
    public sealed record ProjectSettingsDto(
        long Id, Guid? SsoProjectId, string SsoProjectName, string ProjectCode, int EditLeaseMinutes, bool IsActive);
    public sealed record UpsertSettingsDto(Guid? SsoProjectId, string SsoProjectName, string ProjectCode, int EditLeaseMinutes);

    public sealed record ListSettingsQuery : IRequest<Response<IReadOnlyList<ProjectSettingsDto>>>;
    public sealed record UpsertSettingsCommand(UpsertSettingsDto Dto) : IRequest<Response>;

    public sealed class UpsertSettingsValidator : AbstractValidator<UpsertSettingsCommand>
    {
        public UpsertSettingsValidator()
        {
            RuleFor(x => x.Dto.SsoProjectName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Dto.ProjectCode).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Dto.EditLeaseMinutes).GreaterThan(0);
        }
    }

    public sealed class ListSettingsHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<ListSettingsQuery, Response<IReadOnlyList<ProjectSettingsDto>>>
    {
        public async Task<Response<IReadOnlyList<ProjectSettingsDto>>> Handle(ListSettingsQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<ProjectSettingsDto>>(BudgetMessages.M20);
            var rows = await context.BudgetProjectSettings.AsNoTracking().OrderBy(s => s.Id)
                .Select(s => new ProjectSettingsDto(s.Id, s.SsoProjectId, s.SsoProjectName,
                    s.EmployeeChargeProjectCode, s.EditLeaseMinutes, s.IsActive))
                .ToListAsync(ct);
            return new Response<IReadOnlyList<ProjectSettingsDto>> { IsSucced = true, Message = "Project settings", Data = rows };
        }
    }

    /// <summary>Upsert the single active settings row: update it if one exists, else insert one active row.</summary>
    public sealed class UpsertSettingsHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : IRequestHandler<UpsertSettingsCommand, Response>
    {
        public const string ActionCode = "CONFIG_SETTINGS_UPSERT";

        public async Task<Response> Handle(UpsertSettingsCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var dto = request.Dto;

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            var row = await context.BudgetProjectSettings.FirstOrDefaultAsync(s => s.IsActive, ct);
            if (row is null)
            {
                row = new BudgetProjectSettings { IsActive = true };
                context.BudgetProjectSettings.Add(row);
            }
            row.SsoProjectId = dto.SsoProjectId;
            row.SsoProjectName = dto.SsoProjectName.Trim();
            row.EmployeeChargeProjectCode = dto.ProjectCode.Trim();
            row.EditLeaseMinutes = dto.EditLeaseMinutes;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            config.Invalidate();
            return new Response { IsSucced = true, Message = "Project settings saved" };
        }
    }

    // -------------------- roles --------------------
    public sealed record BudgetRoleDto(long Id, string RoleName, string OfficeLevel, bool IsAdmin, bool IsActive);
    public sealed record UpsertRoleDto(string RoleName, string OfficeLevel, bool IsAdmin, bool IsActive);

    public sealed record ListRolesQuery : IRequest<Response<IReadOnlyList<BudgetRoleDto>>>;
    public sealed record UpsertRoleCommand(UpsertRoleDto Dto) : IRequest<Response>;

    public sealed class UpsertRoleValidator : AbstractValidator<UpsertRoleCommand>
    {
        public UpsertRoleValidator()
        {
            RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Dto.OfficeLevel).Must(l => OfficeLevel.All.Contains(l))
                .WithMessage($"officeLevel must be one of {string.Join(", ", OfficeLevel.All)}.");
        }
    }

    public sealed class ListRolesHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<ListRolesQuery, Response<IReadOnlyList<BudgetRoleDto>>>
    {
        public async Task<Response<IReadOnlyList<BudgetRoleDto>>> Handle(ListRolesQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<BudgetRoleDto>>(BudgetMessages.M20);
            var rows = await context.BudgetRoles.AsNoTracking().OrderBy(r => r.RoleName)
                .Select(r => new BudgetRoleDto(r.Id, r.RoleName, r.OfficeLevel, r.IsAdmin, r.IsActive))
                .ToListAsync(ct);
            return new Response<IReadOnlyList<BudgetRoleDto>> { IsSucced = true, Message = "Budget roles", Data = rows };
        }
    }

    public sealed class UpsertRoleHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : IRequestHandler<UpsertRoleCommand, Response>
    {
        public const string ActionCode = "CONFIG_ROLE_UPSERT";

        public async Task<Response> Handle(UpsertRoleCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var dto = request.Dto;

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            var row = await context.BudgetRoles.FirstOrDefaultAsync(r => r.RoleName == dto.RoleName, ct);
            if (row is null)
            {
                row = new BudgetRole { RoleName = dto.RoleName.Trim() };
                context.BudgetRoles.Add(row);
            }
            row.OfficeLevel = dto.OfficeLevel;
            row.IsAdmin = dto.IsAdmin;
            row.IsActive = dto.IsActive;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            config.Invalidate();
            return new Response { IsSucced = true, Message = "Budget role saved" };
        }
    }

    // -------------------- rights (role -> right grant/revoke) --------------------
    public sealed record RightGrantDto(string RoleName, string RightKey, bool Grant);
    public sealed record ListRightsItemDto(string RightKey, IReadOnlyList<string> Roles);
    public sealed record ListRightsQuery : IRequest<Response<IReadOnlyList<ListRightsItemDto>>>;
    public sealed record UpsertRightCommand(RightGrantDto Dto) : IRequest<Response>;

    public sealed class UpsertRightValidator : AbstractValidator<UpsertRightCommand>
    {
        public UpsertRightValidator()
        {
            RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Dto.RightKey).NotEmpty().MaximumLength(100);
        }
    }

    public sealed class ListRightsHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<ListRightsQuery, Response<IReadOnlyList<ListRightsItemDto>>>
    {
        public async Task<Response<IReadOnlyList<ListRightsItemDto>>> Handle(ListRightsQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<ListRightsItemDto>>(BudgetMessages.M20);
            var rights = await context.BudgetRights.AsNoTracking().OrderBy(r => r.RightKey).ToListAsync(ct);
            var pairs = await (
                from rr in context.BudgetRoleRights.AsNoTracking()
                join role in context.BudgetRoles.AsNoTracking() on rr.RoleId equals role.Id
                select new { rr.RightId, role.RoleName }).ToListAsync(ct);
            var byRight = pairs.GroupBy(p => p.RightId).ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).OrderBy(n => n).ToList());
            var data = rights.Select(r => new ListRightsItemDto(r.RightKey,
                byRight.TryGetValue(r.Id, out var roles) ? roles : [])).ToList();
            return new Response<IReadOnlyList<ListRightsItemDto>> { IsSucced = true, Message = "Role rights", Data = data };
        }
    }

    public sealed class UpsertRightHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : IRequestHandler<UpsertRightCommand, Response>
    {
        public const string ActionCode = "CONFIG_ROLE_RIGHT_UPSERT";

        public async Task<Response> Handle(UpsertRightCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var dto = request.Dto;

            var role = await context.BudgetRoles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleName == dto.RoleName, ct);
            var right = await context.BudgetRights.AsNoTracking().FirstOrDefaultAsync(r => r.RightKey == dto.RightKey, ct);
            // Unknown role or right on a grant fails rather than silently creating (design "validation").
            if (role is null || right is null)
                return BudgetResponses.Fail(BudgetMessages.M05);

            var existing = await context.BudgetRoleRights
                .FirstOrDefaultAsync(x => x.RoleId == role.Id && x.RightId == right.Id, ct);
            if (dto.Grant && existing is not null)
                return new Response { IsSucced = true, Message = "Right already granted" };
            if (!dto.Grant && existing is null)
                return new Response { IsSucced = true, Message = "Right already absent" };

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            if (dto.Grant)
                context.BudgetRoleRights.Add(new BudgetRoleRight { RoleId = role.Id, RightId = right.Id });
            else
                context.BudgetRoleRights.Remove(existing!);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            config.Invalidate();
            return new Response { IsSucced = true, Message = dto.Grant ? "Right granted" : "Right revoked" };
        }
    }

    // -------------------- query fields (role -> field grant/revoke) --------------------
    public sealed record QueryFieldGrantDto(string RoleName, string FieldName, bool Grant);
    public sealed record ListQueryFieldsItemDto(string FieldName, IReadOnlyList<string> Roles);
    public sealed record ListQueryFieldsQuery : IRequest<Response<IReadOnlyList<ListQueryFieldsItemDto>>>;
    public sealed record UpsertQueryFieldCommand(QueryFieldGrantDto Dto) : IRequest<Response>;

    public sealed class UpsertQueryFieldValidator : AbstractValidator<UpsertQueryFieldCommand>
    {
        public UpsertQueryFieldValidator()
        {
            RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Dto.FieldName).NotEmpty().MaximumLength(100);
        }
    }

    public sealed class ListQueryFieldsHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<ListQueryFieldsQuery, Response<IReadOnlyList<ListQueryFieldsItemDto>>>
    {
        public async Task<Response<IReadOnlyList<ListQueryFieldsItemDto>>> Handle(ListQueryFieldsQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<ListQueryFieldsItemDto>>(BudgetMessages.M20);
            var fields = await context.BudgetQueryFields.AsNoTracking().OrderBy(f => f.FieldName).ToListAsync(ct);
            var pairs = await (
                from rq in context.BudgetRoleQueryFields.AsNoTracking()
                join role in context.BudgetRoles.AsNoTracking() on rq.RoleId equals role.Id
                select new { rq.QueryFieldId, role.RoleName }).ToListAsync(ct);
            var byField = pairs.GroupBy(p => p.QueryFieldId).ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).OrderBy(n => n).ToList());
            var data = fields.Select(f => new ListQueryFieldsItemDto(f.FieldName,
                byField.TryGetValue(f.Id, out var roles) ? roles : [])).ToList();
            return new Response<IReadOnlyList<ListQueryFieldsItemDto>> { IsSucced = true, Message = "Role query fields", Data = data };
        }
    }

    public sealed class UpsertQueryFieldHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : IRequestHandler<UpsertQueryFieldCommand, Response>
    {
        public const string ActionCode = "CONFIG_ROLE_QUERY_FIELD_UPSERT";

        public async Task<Response> Handle(UpsertQueryFieldCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var dto = request.Dto;

            var role = await context.BudgetRoles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleName == dto.RoleName, ct);
            var field = await context.BudgetQueryFields.AsNoTracking().FirstOrDefaultAsync(f => f.FieldName == dto.FieldName, ct);
            if (role is null || field is null)
                return BudgetResponses.Fail(BudgetMessages.M05);

            var existing = await context.BudgetRoleQueryFields
                .FirstOrDefaultAsync(x => x.RoleId == role.Id && x.QueryFieldId == field.Id, ct);
            if (dto.Grant && existing is not null)
                return new Response { IsSucced = true, Message = "Field already granted" };
            if (!dto.Grant && existing is null)
                return new Response { IsSucced = true, Message = "Field already absent" };

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            if (dto.Grant)
                context.BudgetRoleQueryFields.Add(new BudgetRoleQueryField { RoleId = role.Id, QueryFieldId = field.Id });
            else
                context.BudgetRoleQueryFields.Remove(existing!);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            config.Invalidate();
            return new Response { IsSucced = true, Message = dto.Grant ? "Field granted" : "Field revoked" };
        }
    }

    public sealed class ConfigEndpoints : IEndpoint
    {
        private const string Policy = "BudgetRight:Settings.Admin";

        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-config/settings/list",
                    async (IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new ListSettingsQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListBudgetConfigSettings").WithTags("BudgetConfig")
                .Produces<Response<IReadOnlyList<ProjectSettingsDto>>>(StatusCodes.Status200OK);
            app.MapPost("/api/budget-config/settings/upsert",
                    async (UpsertSettingsDto dto, IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new UpsertSettingsCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("UpsertBudgetConfigSettings").WithTags("BudgetConfig")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-config/roles/list",
                    async (IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new ListRolesQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListBudgetConfigRoles").WithTags("BudgetConfig")
                .Produces<Response<IReadOnlyList<BudgetRoleDto>>>(StatusCodes.Status200OK);
            app.MapPost("/api/budget-config/roles/upsert",
                    async (UpsertRoleDto dto, IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new UpsertRoleCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("UpsertBudgetConfigRole").WithTags("BudgetConfig")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-config/rights/list",
                    async (IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new ListRightsQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListBudgetConfigRights").WithTags("BudgetConfig")
                .Produces<Response<IReadOnlyList<ListRightsItemDto>>>(StatusCodes.Status200OK);
            app.MapPost("/api/budget-config/rights/upsert",
                    async (RightGrantDto dto, IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new UpsertRightCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("UpsertBudgetConfigRight").WithTags("BudgetConfig")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-config/query-fields/list",
                    async (IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new ListQueryFieldsQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListBudgetConfigQueryFields").WithTags("BudgetConfig")
                .Produces<Response<IReadOnlyList<ListQueryFieldsItemDto>>>(StatusCodes.Status200OK);
            app.MapPost("/api/budget-config/query-fields/upsert",
                    async (QueryFieldGrantDto dto, IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new UpsertQueryFieldCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("UpsertBudgetConfigQueryField").WithTags("BudgetConfig")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}

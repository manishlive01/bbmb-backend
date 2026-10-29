using BBMB.BudgetService.Authorization.RoleSource;
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
    // Role configuration (dynamic Budget role management). The list shape is the admin UI contract; a role is
    // "configured" exactly when the config snapshot would authorise it (BudgetRoleLevel.IsAuthorisable, D2).
    public sealed record BudgetRoleConfigDto(
        long RoleId, Guid? SsoRoleId, string RoleName, bool IsActive, bool SsoIsActive, bool IsAdmin,
        int? OfficeLevelId, string? OfficeLevelName, bool IsConfigured,
        IReadOnlyList<string> RightKeys, IReadOnlyList<string> QueryFieldNames, DateTimeOffset? LastSyncedAt);
    /// <summary>Legacy upsert by name: <c>OfficeLevel</c> is a Budget office level code, mapped to its levels row.</summary>
    public sealed record UpsertRoleDto(string RoleName, string OfficeLevel, bool IsAdmin, bool IsActive);
    public sealed record ConfigureRoleDto(long RoleId, int OfficeLevelId, IReadOnlyList<string> RightKeys, IReadOnlyList<string> QueryFieldNames);
    public sealed record LevelOptionDto(int LevelId, string Name);
    public sealed record RightCatalogItemDto(string RightKey, string? Description);
    public sealed record QueryFieldCatalogItemDto(string FieldName, string? Label);

    public sealed record ListRolesQuery : IRequest<Response<IReadOnlyList<BudgetRoleConfigDto>>>;
    public sealed record UpsertRoleCommand(UpsertRoleDto Dto) : IRequest<Response>;
    public sealed record SyncRolesCommand : IRequest<Response<RoleSyncResultDto>>;
    public sealed record ConfigureRoleCommand(ConfigureRoleDto Dto) : IRequest<Response<BudgetRoleConfigDto>>;
    public sealed record ListLevelsQuery : IRequest<Response<IReadOnlyList<LevelOptionDto>>>;
    public sealed record RightCatalogQuery : IRequest<Response<IReadOnlyList<RightCatalogItemDto>>>;
    public sealed record QueryFieldCatalogQuery : IRequest<Response<IReadOnlyList<QueryFieldCatalogItemDto>>>;

    public sealed class UpsertRoleValidator : AbstractValidator<UpsertRoleCommand>
    {
        public UpsertRoleValidator()
        {
            RuleFor(x => x.Dto.RoleName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Dto.OfficeLevel).Must(l => OfficeLevel.All.Contains(l))
                .WithMessage($"officeLevel must be one of {string.Join(", ", OfficeLevel.All)}.");
        }
    }

    public sealed class ConfigureRoleValidator : AbstractValidator<ConfigureRoleCommand>
    {
        public ConfigureRoleValidator()
        {
            RuleFor(x => x.Dto.RoleId).GreaterThan(0).OverridePropertyName("roleId");
            RuleFor(x => x.Dto.OfficeLevelId).GreaterThan(0).OverridePropertyName("officeLevelId");
            RuleFor(x => x.Dto.RightKeys).NotNull().OverridePropertyName("rightKeys");
            RuleForEach(x => x.Dto.RightKeys).NotEmpty().MaximumLength(100).OverridePropertyName("rightKeys");
            RuleFor(x => x.Dto.QueryFieldNames).NotNull().OverridePropertyName("queryFieldNames");
            RuleForEach(x => x.Dto.QueryFieldNames).NotEmpty().MaximumLength(100).OverridePropertyName("queryFieldNames");
        }
    }

    /// <summary>Reads roles in the list shape. Rights and fields are the active grants of active catalogue rows.</summary>
    public static class RoleConfigReader
    {
        public static async Task<List<BudgetRoleConfigDto>> ReadAsync(BudgetContext context, long? roleId, CancellationToken ct)
        {
            var roleQuery = context.BudgetRoles.AsNoTracking();
            if (roleId is { } id)
                roleQuery = roleQuery.Where(r => r.Id == id);
            var roles = await roleQuery
                .Select(r => new
                {
                    r.Id, r.SsoRoleId, r.RoleName, r.IsActive, r.SsoIsActive, r.IsAdmin, r.OfficeLevelId, r.LastSyncedAt,
                    LevelName = r.OfficeLevel != null ? r.OfficeLevel.Name : null,
                    LevelActive = r.OfficeLevel != null && r.OfficeLevel.Status,
                })
                .OrderBy(r => r.RoleName)
                .ToListAsync(ct);
            var ids = roles.Select(r => r.Id).ToList();
            var rights = await (
                from rr in context.BudgetRoleRights.AsNoTracking()
                join right in context.BudgetRights.AsNoTracking() on rr.RightId equals right.Id
                where rr.IsActive && right.IsActive && ids.Contains(rr.RoleId)
                select new { rr.RoleId, right.RightKey }).ToListAsync(ct);
            var fields = await (
                from rq in context.BudgetRoleQueryFields.AsNoTracking()
                join field in context.BudgetQueryFields.AsNoTracking() on rq.QueryFieldId equals field.Id
                where rq.IsActive && field.IsActive && ids.Contains(rq.RoleId)
                select new { rq.RoleId, field.FieldName }).ToListAsync(ct);
            var rightsByRole = rights.ToLookup(x => x.RoleId, x => x.RightKey);
            var fieldsByRole = fields.ToLookup(x => x.RoleId, x => x.FieldName);
            return roles.Select(r =>
            {
                var keys = rightsByRole[r.Id].OrderBy(k => k, StringComparer.Ordinal).ToList();
                var code = BudgetRoleLevel.ToOfficeLevelCode(r.LevelName, r.LevelActive);
                return new BudgetRoleConfigDto(
                    r.Id, r.SsoRoleId, r.RoleName, r.IsActive, r.SsoIsActive, r.IsAdmin, r.OfficeLevelId, r.LevelName,
                    BudgetRoleLevel.IsAuthorisable(r.IsActive, r.SsoIsActive, code, keys.Count > 0),
                    keys, fieldsByRole[r.Id].OrderBy(f => f, StringComparer.Ordinal).ToList(), r.LastSyncedAt);
            }).ToList();
        }
    }

    public sealed class ListRolesHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<ListRolesQuery, Response<IReadOnlyList<BudgetRoleConfigDto>>>
    {
        public async Task<Response<IReadOnlyList<BudgetRoleConfigDto>>> Handle(ListRolesQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<BudgetRoleConfigDto>>(BudgetMessages.M20);
            var rows = await RoleConfigReader.ReadAsync(context, null, ct);
            return new Response<IReadOnlyList<BudgetRoleConfigDto>> { IsSucced = true, Message = "Budget roles", Data = rows };
        }
    }

    /// <summary>
    /// Legacy upsert by role name (kept working). The office level code is mapped to its <c>levels</c> row through
    /// <see cref="BudgetRoleLevel"/>; no such row ⇒ CONFIG_LEVEL_NOT_BUDGET. A role created here has no SSO id
    /// until a sync adopts it by name.
    /// </summary>
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

            var levels = await context.Levels.AsNoTracking().Where(l => l.Status).OrderBy(l => l.Id)
                .Select(l => new { l.Id, l.Name }).ToListAsync(ct);
            var level = levels.FirstOrDefault(l =>
                string.Equals(BudgetRoleLevel.ToOfficeLevelCode(l.Name, true), dto.OfficeLevel, StringComparison.Ordinal));
            if (level is null)
                return BudgetResponses.Fail(BudgetMessages.ConfigLevelNotBudget);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            var row = await context.BudgetRoles.FirstOrDefaultAsync(r => r.RoleName == dto.RoleName, ct);
            if (row is null)
            {
                row = new BudgetRole { RoleName = dto.RoleName.Trim() };
                context.BudgetRoles.Add(row);
            }
            row.OfficeLevelId = level.Id;
            row.IsAdmin = dto.IsAdmin;
            row.IsActive = dto.IsActive;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            config.Invalidate();
            return new Response { IsSucced = true, Message = "Budget role saved" };
        }
    }

    /// <summary>
    /// On-demand Auth SSO role sync (D3). Reads the Budget project's roles with the calling admin's bearer token,
    /// then merges them in one audited action scope through <see cref="BudgetRoleSync.Plan"/>. Never touches
    /// Budget-owned columns or rows. An SSO failure, or an empty role list, changes nothing (an empty list would
    /// otherwise deactivate every role, including the administrator's own, and lock Budget administration out).
    /// </summary>
    public sealed class SyncRolesHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config,
        ISsoRoleClient ssoRoles, IHttpContextAccessor httpContextAccessor, ILogger<SyncRolesHandler> logger,
        TimeProvider? clock = null)
        : IRequestHandler<SyncRolesCommand, Response<RoleSyncResultDto>>
    {
        public const string ActionCode = "CONFIG_ROLE_SYNC";

        public async Task<Response<RoleSyncResultDto>> Handle(SyncRolesCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<RoleSyncResultDto>(BudgetMessages.M20);

            var projectId = await context.BudgetProjectSettings.AsNoTracking()
                .Where(s => s.IsActive).Select(s => s.SsoProjectId).FirstOrDefaultAsync(ct);
            if (projectId is null)
                return BudgetResponses.Fail<RoleSyncResultDto>(BudgetMessages.SsoProjectUnset);

            var token = BearerToken(httpContextAccessor.HttpContext);
            if (token is null)
                return BudgetResponses.Fail<RoleSyncResultDto>(BudgetMessages.M20);

            // The HTTP call is made before the transaction opens, so no DB transaction waits on Auth SSO.
            var fetched = await ssoRoles.GetProjectRolesAsync(projectId.Value, token, ct);
            if (fetched.Roles is null || fetched.Roles.Count == 0)
            {
                logger.LogWarning("Budget role sync refused: {Reason}", fetched.Error ?? "EMPTY_ROLE_LIST");
                return BudgetResponses.Fail<RoleSyncResultDto>(BudgetMessages.SsoSyncFailed);
            }

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            var existing = await context.BudgetRoles.ToListAsync(ct);
            var plan = BudgetRoleSync.Plan(existing, fetched.Roles, (clock ?? TimeProvider.System).GetUtcNow());
            if (plan.SkippedNames.Count > 0)
                logger.LogWarning("Budget role sync skipped {Count} SSO role(s) whose name belongs to another Budget role: {Names}",
                    plan.SkippedNames.Count, string.Join(", ", plan.SkippedNames));
            if (plan.HasChanges)
            {
                context.BudgetRoles.AddRange(plan.NewRoles);
                await context.SaveChangesAsync(ct);
            }
            await tx.CommitAsync(ct);
            if (plan.HasChanges)
                config.Invalidate();
            return new Response<RoleSyncResultDto> { IsSucced = true, Message = "Budget roles synchronised", Data = plan.Counts };
        }

        private static string? BearerToken(HttpContext? http)
        {
            var header = http?.Request.Headers.Authorization.ToString();
            const string prefix = "Bearer ";
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;
            var token = header[prefix.Length..].Trim();
            return token.Length == 0 ? null : token;
        }
    }

    /// <summary>
    /// Replace-set configuration of one role: its office level (a <c>levels</c> row that is a Budget office level),
    /// and exactly the listed rights and query fields. Listed grants are added or reactivated; active grants not
    /// listed are soft-deactivated (the tables forbid DELETE). One audited transaction; the cache is invalidated.
    /// An inactive or SSO-inactive role is refused. A change that would leave the caller's own acting role unable to
    /// administer (not an effective admin and no Settings.Admin, or no right at all) is refused, so an administrator
    /// cannot lock Budget administration out through this screen.
    /// </summary>
    public sealed class ConfigureRoleHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetConfigProvider config)
        : IRequestHandler<ConfigureRoleCommand, Response<BudgetRoleConfigDto>>
    {
        public const string ActionCode = "CONFIG_ROLE_CONFIGURE";
        public const string AdminRightKey = "Settings.Admin";

        public async Task<Response<BudgetRoleConfigDto>> Handle(ConfigureRoleCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<BudgetRoleConfigDto>(BudgetMessages.M20);
            var dto = request.Dto;
            var wantedRights = dto.RightKeys.Select(k => k.Trim()).Distinct(StringComparer.Ordinal).ToList();
            var wantedFields = dto.QueryFieldNames.Select(f => f.Trim()).Distinct(StringComparer.Ordinal).ToList();

            var role = await context.BudgetRoles.FirstOrDefaultAsync(r => r.Id == dto.RoleId, ct);
            if (role is null)
                return BudgetResponses.Fail<BudgetRoleConfigDto>(BudgetMessages.NotFound);
            if (!role.IsActive || !role.SsoIsActive)
                return BudgetResponses.Fail<BudgetRoleConfigDto>(BudgetMessages.RoleInactive);

            var level = await context.Levels.AsNoTracking()
                .Where(l => l.Id == dto.OfficeLevelId && l.Status).Select(l => new { l.Id, l.Name }).FirstOrDefaultAsync(ct);
            if (level is null)
                return BudgetResponses.Fail<BudgetRoleConfigDto>(BudgetMessages.ConfigInvalid);
            if (BudgetRoleLevel.ToOfficeLevelCode(level.Name, true) is null)
                return BudgetResponses.Fail<BudgetRoleConfigDto>(BudgetMessages.ConfigLevelNotBudget);

            var rights = await context.BudgetRights.AsNoTracking()
                .Where(r => r.IsActive && wantedRights.Contains(r.RightKey)).Select(r => r.Id).ToListAsync(ct);
            var fields = await context.BudgetQueryFields.AsNoTracking()
                .Where(f => f.IsActive && wantedFields.Contains(f.FieldName)).Select(f => f.Id).ToListAsync(ct);
            if (rights.Count != wantedRights.Count || fields.Count != wantedFields.Count)
                return BudgetResponses.Fail<BudgetRoleConfigDto>(BudgetMessages.ConfigInvalid);

            if (string.Equals(role.RoleName, actor.RoleCode, StringComparison.OrdinalIgnoreCase))
            {
                var snapshot = await config.GetAsync(ct);
                var effectiveAdmin = snapshot.AdminRoles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase);
                if (wantedRights.Count == 0 || (!effectiveAdmin && !wantedRights.Contains(AdminRightKey, StringComparer.Ordinal)))
                    return BudgetResponses.Fail<BudgetRoleConfigDto>(BudgetMessages.ConfigSelfLockout);
            }

            await using (var tx = await scope.BeginAsync(ActionCode, ct: ct))
            {
                role.OfficeLevelId = level.Id;

                var wantedRightIds = rights.ToHashSet();
                var roleRights = await context.BudgetRoleRights.Where(x => x.RoleId == role.Id).ToListAsync(ct);
                foreach (var rr in roleRights)
                {
                    var keep = wantedRightIds.Contains(rr.RightId);
                    if (rr.IsActive != keep)
                        rr.IsActive = keep;
                }
                foreach (var id in wantedRightIds.Where(id => roleRights.All(x => x.RightId != id)))
                    context.BudgetRoleRights.Add(new BudgetRoleRight { RoleId = role.Id, RightId = id, IsActive = true });

                var wantedFieldIds = fields.ToHashSet();
                var roleFields = await context.BudgetRoleQueryFields.Where(x => x.RoleId == role.Id).ToListAsync(ct);
                foreach (var rq in roleFields)
                {
                    var keep = wantedFieldIds.Contains(rq.QueryFieldId);
                    if (rq.IsActive != keep)
                        rq.IsActive = keep;
                }
                foreach (var id in wantedFieldIds.Where(id => roleFields.All(x => x.QueryFieldId != id)))
                    context.BudgetRoleQueryFields.Add(new BudgetRoleQueryField { RoleId = role.Id, QueryFieldId = id, IsActive = true });

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            config.Invalidate();

            var row = (await RoleConfigReader.ReadAsync(context, role.Id, ct)).Single();
            return new Response<BudgetRoleConfigDto> { IsSucced = true, Message = "Budget role configured", Data = row };
        }
    }

    public sealed class ListLevelsHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<ListLevelsQuery, Response<IReadOnlyList<LevelOptionDto>>>
    {
        public async Task<Response<IReadOnlyList<LevelOptionDto>>> Handle(ListLevelsQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<LevelOptionDto>>(BudgetMessages.M20);
            var rows = await context.Levels.AsNoTracking().Where(l => l.Status).OrderBy(l => l.Name)
                .Select(l => new LevelOptionDto(l.Id, l.Name)).ToListAsync(ct);
            return new Response<IReadOnlyList<LevelOptionDto>> { IsSucced = true, Message = "Levels", Data = rows };
        }
    }

    /// <summary>Active right keys. <c>budget_right</c> has no description column, so Description is null.</summary>
    public sealed class RightCatalogHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<RightCatalogQuery, Response<IReadOnlyList<RightCatalogItemDto>>>
    {
        public async Task<Response<IReadOnlyList<RightCatalogItemDto>>> Handle(RightCatalogQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<RightCatalogItemDto>>(BudgetMessages.M20);
            var rows = await context.BudgetRights.AsNoTracking().Where(r => r.IsActive).OrderBy(r => r.RightKey)
                .Select(r => new RightCatalogItemDto(r.RightKey, null)).ToListAsync(ct);
            return new Response<IReadOnlyList<RightCatalogItemDto>> { IsSucced = true, Message = "Rights catalogue", Data = rows };
        }
    }

    /// <summary>Active query fields. <c>budget_query_field</c> has no label column, so Label is null.</summary>
    public sealed class QueryFieldCatalogHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<QueryFieldCatalogQuery, Response<IReadOnlyList<QueryFieldCatalogItemDto>>>
    {
        public async Task<Response<IReadOnlyList<QueryFieldCatalogItemDto>>> Handle(QueryFieldCatalogQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<IReadOnlyList<QueryFieldCatalogItemDto>>(BudgetMessages.M20);
            var rows = await context.BudgetQueryFields.AsNoTracking().Where(f => f.IsActive).OrderBy(f => f.FieldName)
                .Select(f => new QueryFieldCatalogItemDto(f.FieldName, null)).ToListAsync(ct);
            return new Response<IReadOnlyList<QueryFieldCatalogItemDto>> { IsSucced = true, Message = "Query fields catalogue", Data = rows };
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
                from rr in context.BudgetRoleRights.AsNoTracking().Where(x => x.IsActive)
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

            // Revoke is a soft deactivation (is_active = false), not a physical DELETE: the config tables are
            // append-only and carry a BEFORE DELETE forbid trigger, and a DELETE would also lose the revoke
            // from the audit trail. The snapshot reads active rows only. A re-grant reactivates the row.
            var existing = await context.BudgetRoleRights
                .FirstOrDefaultAsync(x => x.RoleId == role.Id && x.RightId == right.Id, ct);
            if (dto.Grant && existing is { IsActive: true })
                return new Response { IsSucced = true, Message = "Right already granted" };
            if (!dto.Grant && existing is not { IsActive: true })
                return new Response { IsSucced = true, Message = "Right already absent" };

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            if (existing is null)
                context.BudgetRoleRights.Add(new BudgetRoleRight { RoleId = role.Id, RightId = right.Id });
            else
                existing.IsActive = dto.Grant;
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
                from rq in context.BudgetRoleQueryFields.AsNoTracking().Where(x => x.IsActive)
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

            // Revoke is a soft deactivation (is_active = false), not a physical DELETE — same reasoning as the
            // right grant/revoke above: the table is append-only with a DELETE-forbid trigger, and the
            // snapshot reads active rows only. A re-grant reactivates the row.
            var existing = await context.BudgetRoleQueryFields
                .FirstOrDefaultAsync(x => x.RoleId == role.Id && x.QueryFieldId == field.Id, ct);
            if (dto.Grant && existing is { IsActive: true })
                return new Response { IsSucced = true, Message = "Field already granted" };
            if (!dto.Grant && existing is not { IsActive: true })
                return new Response { IsSucced = true, Message = "Field already absent" };

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            if (existing is null)
                context.BudgetRoleQueryFields.Add(new BudgetRoleQueryField { RoleId = role.Id, QueryFieldId = field.Id });
            else
                existing.IsActive = dto.Grant;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            config.Invalidate();
            return new Response { IsSucced = true, Message = dto.Grant ? "Field granted" : "Field revoked" };
        }
    }

    public sealed class ConfigEndpoints : IEndpoint
    {
        private const string Policy = "BudgetRight:Settings.Admin";

        /// <summary>
        /// The shared status mapping, plus the two the role-configuration contract adds: an invalid level, right or
        /// field is 400; an Auth SSO failure is 502. Bodies keep the Response/messageId shape.
        /// </summary>
        public static IResult ConfigResults<TResponse>(TResponse response) where TResponse : Response =>
            response.IsSucced ? BudgetResults.Ok(response) : response.MessageId switch
            {
                BudgetMessages.ConfigInvalid or BudgetMessages.ConfigLevelNotBudget => Results.BadRequest((object)response),
                BudgetMessages.SsoSyncFailed => Results.Json((object)response, statusCode: StatusCodes.Status502BadGateway),
                _ => BudgetResults.Ok(response),
            };

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
                .Produces<Response<IReadOnlyList<BudgetRoleConfigDto>>>(StatusCodes.Status200OK);
            app.MapPost("/api/budget-config/roles/upsert",
                    async (UpsertRoleDto dto, IMediator m, CancellationToken ct) => ConfigResults(await m.Send(new UpsertRoleCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("UpsertBudgetConfigRole").WithTags("BudgetConfig")
                .Produces<Response>(StatusCodes.Status200OK).Produces<Response>(StatusCodes.Status409Conflict);

            app.MapPost("/api/budget-config/roles/sync",
                    async (IMediator m, CancellationToken ct) => ConfigResults(await m.Send(new SyncRolesCommand(), ct)))
                .RequireAuthorization(Policy).WithName("SyncBudgetConfigRoles").WithTags("BudgetConfig")
                .Produces<Response<RoleSyncResultDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status409Conflict).Produces<Response>(StatusCodes.Status502BadGateway);
            app.MapPost("/api/budget-config/roles/configure",
                    async (ConfigureRoleDto dto, IMediator m, CancellationToken ct) => ConfigResults(await m.Send(new ConfigureRoleCommand(dto), ct)))
                .RequireAuthorization(Policy).WithName("ConfigureBudgetConfigRole").WithTags("BudgetConfig")
                .Produces<Response<BudgetRoleConfigDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status400BadRequest).Produces<Response>(StatusCodes.Status409Conflict);
            app.MapPost("/api/budget-config/levels/list",
                    async (IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new ListLevelsQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListBudgetConfigLevels").WithTags("BudgetConfig")
                .Produces<Response<IReadOnlyList<LevelOptionDto>>>(StatusCodes.Status200OK);
            app.MapPost("/api/budget-config/rights/catalog",
                    async (IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new RightCatalogQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListBudgetConfigRightCatalog").WithTags("BudgetConfig")
                .Produces<Response<IReadOnlyList<RightCatalogItemDto>>>(StatusCodes.Status200OK);
            app.MapPost("/api/budget-config/query-fields/catalog",
                    async (IMediator m, CancellationToken ct) => BudgetResults.Ok(await m.Send(new QueryFieldCatalogQuery(), ct)))
                .RequireAuthorization(Policy).WithName("ListBudgetConfigQueryFieldCatalog").WithTags("BudgetConfig")
                .Produces<Response<IReadOnlyList<QueryFieldCatalogItemDto>>>(StatusCodes.Status200OK);

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

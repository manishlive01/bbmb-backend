using Auth.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields.Dtos;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields
{
    /// <summary>
    /// Design §8.8 (F-26, BUD-220, BUD-221). Actor required (M-20). Returns the whitelisted field catalogue
    /// (business names only; no column or table name leaves the service), narrowed to the fields the acting
    /// role may use under <c>BudgetRights:QueryFieldRoles</c> (configuration, BUD-033). RunQuery applies the
    /// same narrowing, so a field hidden here is also refused there. The role → field map itself is still an
    /// open item with BBMB (OI-26); the shipped default grants every field to every Budget.View role.
    /// </summary>
    public sealed class GetQueryFieldsHandler(IBudgetActorSource actorSource, IOptionsMonitor<BudgetRightsOptions> rights)
        : IRequestHandler<GetQueryFieldsQuery, Response<List<QueryFieldDto>>>
    {
        public async Task<Response<List<QueryFieldDto>>> Handle(GetQueryFieldsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<QueryFieldDto>>(BudgetMessages.M20);
            var fields = QueryFieldCatalogue.ForRole(actor.RoleCode, rights.CurrentValue.QueryFieldRoles)
                .Select(f => new QueryFieldDto(f.Name, f.Kind.ToString(), f.Description))
                .ToList();
            return new Response<List<QueryFieldDto>> { IsSucced = true, Message = "Query fields", Data = fields };
        }
    }
}

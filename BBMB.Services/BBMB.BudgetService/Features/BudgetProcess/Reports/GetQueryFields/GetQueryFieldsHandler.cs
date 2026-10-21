using BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields.Dtos;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields
{
    /// <summary>
    /// Design §8.8 (F-26, BUD-220, BUD-221). Actor required (M-20). Returns the whitelisted field catalogue
    /// (business names only; no column or table name leaves the service). Role-based narrowing of the catalogue
    /// is an open item (OI-26: the role→field map is not yet confirmed); until then every Budget.View reader
    /// receives the full catalogue, which the RunQuery whitelist and row scoping still constrain.
    /// </summary>
    public sealed class GetQueryFieldsHandler(IBudgetActorSource actorSource)
        : IRequestHandler<GetQueryFieldsQuery, Response<List<QueryFieldDto>>>
    {
        public async Task<Response<List<QueryFieldDto>>> Handle(GetQueryFieldsQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<List<QueryFieldDto>>(BudgetMessages.M20);
            var fields = QueryFieldCatalogue.Fields
                .Select(f => new QueryFieldDto(f.Name, f.Kind.ToString(), f.Description))
                .ToList();
            return new Response<List<QueryFieldDto>> { IsSucced = true, Message = "Query fields", Data = fields };
        }
    }
}

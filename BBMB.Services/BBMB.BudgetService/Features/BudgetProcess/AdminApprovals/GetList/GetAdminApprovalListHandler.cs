using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetList
{
    /// <summary>Approvals of the offices the actor sees (BUD-067), each with every version (BUD-063).</summary>
    public sealed class GetAdminApprovalListHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetAdminApprovalListQuery, Response<List<AdminApprovalDto>>>
    {
        public async Task<Response<List<AdminApprovalDto>>> Handle(GetAdminApprovalListQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<AdminApprovalDto>>(BudgetMessages.M20);
            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            return new Response<List<AdminApprovalDto>>
            {
                IsSucced = true, Message = "Administrative approvals",
                Data = await AdminApprovalReads.LoadAsync(context, visible.ToList(), null, request.WorkId, ct),
            };
        }
    }
}

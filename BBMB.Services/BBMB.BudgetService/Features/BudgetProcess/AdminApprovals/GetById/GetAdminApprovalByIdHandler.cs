using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetById
{
    /// <summary>One approval with every version; absent or outside the actor's offices → NOT_FOUND (§4.9).</summary>
    public sealed class GetAdminApprovalByIdHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetAdminApprovalByIdQuery, Response<AdminApprovalDto>>
    {
        public async Task<Response<AdminApprovalDto>> Handle(GetAdminApprovalByIdQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<AdminApprovalDto>(BudgetMessages.M20);
            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            var found = (await AdminApprovalReads.LoadAsync(context, visible.ToList(), request.AdminApprovalId, null, ct)).SingleOrDefault();
            return found is null
                ? BudgetResponses.Fail<AdminApprovalDto>(BudgetMessages.NotFound)
                : new Response<AdminApprovalDto> { IsSucced = true, Message = "Administrative approval", Data = found };
        }
    }
}

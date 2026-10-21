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
    public sealed record GetAdminApprovalListQuery(int? WorkId) : IRequest<Response<List<AdminApprovalDto>>>;
}

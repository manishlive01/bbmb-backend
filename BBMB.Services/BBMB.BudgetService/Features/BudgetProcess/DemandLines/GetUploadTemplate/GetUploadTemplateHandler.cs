using BBMB.BudgetService.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetUploadTemplate.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetUploadTemplate
{
    /// <summary>
    /// Design §8.5 BUD-097. Order (the §4.9 read order): no actor → M-20; proposal absent or not visible →
    /// NOT_FOUND; the category is not WORKS → M-19 (upload is a works function, F-08); the F07.Enter right →
    /// M-20. The workbook carries the fourteen F-07 headings in order (<see cref="UploadTemplateColumns.Headings"/>),
    /// the same list the upload checks the file against (BUD-098), and no data rows.
    /// </summary>
    public sealed class GetUploadTemplateHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<GetUploadTemplateQuery, Response<UploadTemplateFileDto>>
    {
        public async Task<Response<UploadTemplateFileDto>> Handle(GetUploadTemplateQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<UploadTemplateFileDto>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<UploadTemplateFileDto>(BudgetMessages.NotFound);
            if (p.Category != ProposalCategory.Works)
                return BudgetResponses.Fail<UploadTemplateFileDto>(BudgetMessages.M19);
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            if (!(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy("F07.Enter"))).Succeeded)
                return BudgetResponses.Fail<UploadTemplateFileDto>(BudgetMessages.M20);

            ExcelPackage.License.SetNonCommercialPersonal("BBMB");
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Works demand");
            for (var i = 0; i < UploadTemplateColumns.Headings.Count; i++)
            {
                var cell = sheet.Cells[1, i + 1];
                cell.Value = UploadTemplateColumns.Headings[i];
                cell.Style.Font.Bold = true;
            }
            var bytes = await package.GetAsByteArrayAsync(ct);
            return new Response<UploadTemplateFileDto>
            {
                IsSucced = true,
                Message = "Upload template",
                Data = new UploadTemplateFileDto(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "works-demand-template.xlsx"),
            };
        }
    }
}

using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.Update
{
    public class UpdateWorkHandler : IRequestHandler<UpdateWorkCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetActionScope _scope;
        public UpdateWorkHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        {
            _context = context;
            _actorSource = actorSource;
            _scope = scope;
        }
        public async Task<Response> Handle(UpdateWorkCommand request, CancellationToken ct)
        {
            // work carries fn_require_actor from P1 (DB-28): every write runs inside the action scope.
            if (await _actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var work = await _context.Works
                .FirstOrDefaultAsync(w => w.WorkCode == request.Code, ct);

            if (work == null)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Work with code '{request.Code}' not found"
                };

            // Validate division exists
            //var divisionExists = await _context.Divisions
            //    .AnyAsync(d => d.Id == request.dto.DivisionId, ct);

            //if (!divisionExists)
            //    return new Response
            //    {
            //        IsSucced = false,
            //        Message = $"Division with code '{request.dto.DivisionId}' not found"
            //    };

            // Validate major head exists
            var majorHeadExists = await _context.MajorHeads
                .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode, ct);

            if (!majorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Major account code '{request.dto.MajorAccCode}' not found"
                };

            // Validate sub-major head exists
            var subMajorHeadExists = await _context.SubMajorHeads
                .AnyAsync(s => s.MajorAccCode == request.dto.MajorAccCode &&
                              s.SubMajorAccCode == request.dto.SubMajorAccCode, ct);

            if (!subMajorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Sub-major account code '{request.dto.SubMajorAccCode}' not found for major code '{request.dto.MajorAccCode}'"
                };

            // Validate minor head exists
            var minorHeadExists = await _context.MinorHeads
                .AnyAsync(m => 
                //m.MajorAccCode == request.dto.MajorAccCode &&
                //              m.SubMajorAccCode == request.dto.SubMajorAccCode &&
                              m.MinorAccCode == request.dto.MinorAccCode, ct);

            if (!minorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Minor account code '{request.dto.MinorAccCode}' not found"
                };

            // Validate sub-minor head exists
            var subMinorHeadExists = await _context.SubMinorHeads
                .AnyAsync(s => 
                //s.MajorAccCode == request.dto.MajorAccCode &&
                //              s.SubMajorAccCode == request.dto.SubMajorAccCode &&
                              s.MinorAccCode == request.dto.MinorAccCode &&
                              s.SubMinorAccCode == request.dto.SubMinorAccCode, ct);

            if (!subMinorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Sub-minor account code '{request.dto.SubMinorAccCode}' not found"
                };

            //// Validate detail head exists
            //var detailHeadExists = await _context.DetailHeads
            //    .AnyAsync(d => d.MajorAccCode == request.dto.MajorAccCode &&
            //                  d.SubMajorAccCode == request.dto.SubMajorAccCode &&
            //                  d.MinorAccCode == request.dto.MinorAccCode &&
            //                  d.SubMinorAccCode == request.dto.SubMinorAccCode &&
            //                  d.DetailAccCode == request.dto.DetailAccCode, ct);

            //if (!detailHeadExists)
            //    return new Response
            //    {
            //        IsSucced = false,
            //        Message = $"Detail account code '{request.dto.DetailAccCode}' not found"
            //    };

            // Validate object classification exists
            var objectClassExists = await _context.ObjectClassifications
                .AnyAsync(o => o.ObjectAccCode == request.dto.ObjectAccCode, ct);

            if (!objectClassExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Object account code '{request.dto.ObjectAccCode}' not found"
                };

            // Validate designation if provided
            if (!string.IsNullOrEmpty(request.dto.DesignationCode))
            {
                var designationExists = await _context.Designations
                    .AnyAsync(d => d.DesignationCode == request.dto.DesignationCode, ct);

                if (!designationExists)
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Designation code '{request.dto.DesignationCode}' not found"
                    };
            }

            // ================= UPDATE ALL FIELDS =================

            work.WorkDesc = request.dto.WorkDesc;
            //work.DivisionCode = request.dto.DivisionCode;
            //work.DivSection = request.dto.DivSection;
            work.MajorAccCode = request.dto.MajorAccCode;
            work.SubMajorAccCode = request.dto.SubMajorAccCode;
            work.MinorAccCode = request.dto.MinorAccCode;
            work.SubMinorAccCode = request.dto.SubMinorAccCode;
            work.DetailAccCode = request.dto.DetailAccCode;
            work.ObjectAccCode = request.dto.ObjectAccCode;

            //work.CECode = request.dto.CECode;
            //work.CRCode = request.dto.CRCode;

            work.OriginalBudget = request.dto.OriginalBudget ?? string.Empty;
            work.RevisedBudget = request.dto.RevisedBudget ?? string.Empty;
            work.ProposedBudget = request.dto.ProposedBudget ?? string.Empty;
            work.Remarks = request.dto.Remarks ?? string.Empty;

            work.UserCode = request.dto.UserCode;
            work.FinYrStart = request.dto.FinYrStart;
            work.FinYrEnd = request.dto.FinYrEnd;
            work.SanctionNo = request.dto.SanctionNo;
            work.SanctionDate = request.dto.SanctionDate;
            work.DesignationCode = request.dto.DesignationCode;
            work.EstimatedCost = request.dto.EstimatedCost;
            work.Status = request.dto.Status;
            work.Nature = request.dto.Nature;
            work.RefWorkCode = request.dto.RefWorkCode;
            await using var tx = await _scope.BeginAsync("WORKS_UPDATE", ct: ct);
            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Work updated successfully"
            };
        }
    }
}

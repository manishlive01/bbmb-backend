using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.Create
{
    public class CreateWorkHandler : IRequestHandler<CreateWorkCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetActionScope _scope;
        public CreateWorkHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        {
            _context = context;
            _actorSource = actorSource;
            _scope = scope;
        }
        public async Task<Response> Handle(CreateWorkCommand request, CancellationToken ct)
        {
            // work carries fn_require_actor from P1 (DB-28): every write runs inside the action scope.
            if (await _actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            // ================= AUTO GENERATE WORK CODE =================

            var divisionId = request.dto.DivisionId;
            var divId= await _context.Divisions
                .Where(d => d.Id == divisionId)
                .Select(d => d.Id)
                .FirstOrDefaultAsync(ct);
            if(divId is 0)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with Id '{divisionId}' not found"
                };
            }
            // Get last work for this division
            var lastWorkCode = await _context.Works
                .Where(w => w.DivisionId == divId)
                .OrderByDescending(w => w.WorkCode)
                .Select(w => w.WorkCode)
                .FirstOrDefaultAsync(ct);

            int nextNumber = 1;

            var lastWorkCodeStr = lastWorkCode.ToString();
            var divisionStr = divisionId.ToString();

            if (!string.IsNullOrEmpty(lastWorkCodeStr) &&
                lastWorkCodeStr.StartsWith(divisionStr) &&
                lastWorkCodeStr.Length > divisionStr.Length)
            {
                var numericPart = lastWorkCodeStr.Substring(divisionStr.Length);

                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Generate new WorkCode
            var newWorkCode = $"{divisionId}{nextNumber.ToString("D5")}";

            request.dto.WorkCode = newWorkCode;

            // ================= VALIDATIONS =================

            var divisionExists = await _context.Divisions.AsNoTracking()
                .AnyAsync(d => d.Id == divisionId, ct);

            if (!divisionExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with code '{divisionId}' not found"
                };

            var majorHeadExists = await _context.MajorHeads.AsNoTracking()
                .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode, ct);

            if (!majorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Major account code '{request.dto.MajorAccCode}' not found"
                };

            var subMajorHeadExists = await _context.SubMajorHeads.AsNoTracking()
                .AnyAsync(s => s.MajorAccCode == request.dto.MajorAccCode &&
                              s.SubMajorAccCode == request.dto.SubMajorAccCode, ct);

            if (!subMajorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Sub-major account code '{request.dto.SubMajorAccCode}' not found"
                };

            var minorHeadExists = await _context.MinorHeads.AsNoTracking()
                .AnyAsync(m => m.MinorAccCode == request.dto.MinorAccCode, ct);

            if (!minorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Minor account code '{request.dto.MinorAccCode}' not found"
                };

            var subMinorHeadExists = await _context.SubMinorHeads.AsNoTracking()
                .AnyAsync(s => s.MinorAccCode == request.dto.MinorAccCode &&
                               s.SubMinorAccCode == request.dto.SubMinorAccCode, ct);

            if (!subMinorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Sub-minor account code '{request.dto.SubMinorAccCode}' not found"
                };

            var objectClassExists = await _context.ObjectClassifications.AsNoTracking()
                .AnyAsync(o => o.ObjectAccCode == request.dto.ObjectAccCode, ct);

            if (!objectClassExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Object account code '{request.dto.ObjectAccCode}' not found"
                };

            if (!string.IsNullOrEmpty(request.dto.DesignationCode))
            {
                var designationExists = await _context.Designations.AsNoTracking()
                    .AnyAsync(d => d.DesignationCode == request.dto.DesignationCode, ct);

                if (!designationExists)
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Designation code '{request.dto.DesignationCode}' not found"
                    };
            }

            // ================= CREATE WORK =================

            var work = request.dto.Adapt<Work>();
            // Set server-side after Adapt, never from the request body (design §9.3, §11.3): a work keyed on
            // the work master screen is a continuing work keyed in the first cycle (DDD T-23, L-08).
            work.EnteredVia = WorkEnteredVia.LegacyOpening;
            work.PreDdd = false;
            await using var tx = await _scope.BeginAsync("WORKS_ENTER", ct: ct);
            await _context.Works.AddAsync(work, ct);
            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = $"Work created successfully with WorkCode {newWorkCode}"
            };
        }
    }
}

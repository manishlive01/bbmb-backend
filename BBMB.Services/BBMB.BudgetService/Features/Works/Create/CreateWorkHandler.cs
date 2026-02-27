using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.Create
{
    public class CreateWorkHandler : IRequestHandler<CreateWorkCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateWorkHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateWorkCommand request, CancellationToken ct)
        {
            // ================= AUTO GENERATE WORK CODE =================

            var divisionCode = request.dto.DivisionCode;

            // Get last work for this division
            var lastWorkCode = await _context.Works
                .Where(w => w.DivisionCode == divisionCode)
                .OrderByDescending(w => w.WorkCode)
                .Select(w => w.WorkCode)
                .FirstOrDefaultAsync(ct);

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastWorkCode))
            {
                // Extract numeric suffix (after division code)
                var numericPart = lastWorkCode.Substring(divisionCode.ToString().Length);

                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Generate new WorkCode
            var newWorkCode = $"{divisionCode}{nextNumber.ToString("D5")}";

            request.dto.WorkCode = newWorkCode;

            // ================= VALIDATIONS =================

            var divisionExists = await _context.Divisions.AsNoTracking()
                .AnyAsync(d => d.DivisionCode == divisionCode, ct);

            if (!divisionExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with code '{divisionCode}' not found"
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

            await _context.Works.AddAsync(work, ct);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = $"Work created successfully with WorkCode {newWorkCode}"
            };
        }
    }
}

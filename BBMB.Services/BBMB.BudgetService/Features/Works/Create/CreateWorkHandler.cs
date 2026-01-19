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
            // Check for duplicate work code
            var exists = await _context.Works
                .AnyAsync(w => w.WorkCode == request.dto.WorkCode, ct);

            if (exists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Work with code '{request.dto.WorkCode}' already exists"
                };

            // Validate division exists
            var divisionExists = await _context.Divisions
                .AnyAsync(d => d.DivisionCode == request.dto.DivisionCode, ct);

            if (!divisionExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with code '{request.dto.DivisionCode}' not found"
                };

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
                .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode &&
                              m.SubMajorAccCode == request.dto.SubMajorAccCode &&
                              m.MinorAccCode == request.dto.MinorAccCode, ct);

            if (!minorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Minor account code '{request.dto.MinorAccCode}' not found"
                };

            // Validate sub-minor head exists
            var subMinorHeadExists = await _context.SubMinorHeads
                .AnyAsync(s => s.MajorAccCode == request.dto.MajorAccCode &&
                              s.SubMajorAccCode == request.dto.SubMajorAccCode &&
                              s.MinorAccCode == request.dto.MinorAccCode &&
                              s.SubMinorAccCode == request.dto.SubMinorAccCode, ct);

            if (!subMinorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Sub-minor account code '{request.dto.SubMinorAccCode}' not found"
                };

            // Validate detail head exists
            var detailHeadExists = await _context.DetailHeads
                .AnyAsync(d => d.MajorAccCode == request.dto.MajorAccCode &&
                              d.SubMajorAccCode == request.dto.SubMajorAccCode &&
                              d.MinorAccCode == request.dto.MinorAccCode &&
                              d.SubMinorAccCode == request.dto.SubMinorAccCode &&
                              d.DetailAccCode == request.dto.DetailAccCode, ct);

            if (!detailHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Detail account code '{request.dto.DetailAccCode}' not found"
                };

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

            // Map DTO to entity and create
            var work = request.dto.Adapt<Work>();

            _context.Works.Add(work);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Work created successfully"
            };
        }
    }
}

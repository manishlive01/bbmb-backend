using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public class CreateMinorHeadHandler : IRequestHandler<CreateMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<CreateMinorHeadHandler> _logger;

        public CreateMinorHeadHandler(BudgetContext context, ILogger<CreateMinorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(CreateMinorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Creating minor head with code: {MinorAccCode}", request.dto.MinorAccCode);

                // Check for duplicate
                var exists = await _context.MinorHeads
                    .AnyAsync(m => m.MinorAccCode == request.dto.MinorAccCode, ct);

                if (exists)
                {
                    _logger.LogWarning("Minor head with code {MinorAccCode} already exists", request.dto.MinorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor head with code '{request.dto.MinorAccCode}' already exists"
                    };
                }

                //// Check if major head exists
                //var majorHeadExists = await _context.MajorHeads
                //    .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode, ct);

                //if (!majorHeadExists)
                //{
                //    _logger.LogWarning("Major head with code {MajorAccCode} does not exist", request.dto.MajorAccCode);
                //    return new Response
                //    {
                //        IsSucced = false,
                //        Message = $"Major head with code '{request.dto.MajorAccCode}' does not exist"
                //    };
                //}

                //// Check if sub-major head exists
                //var subMajorHeadExists = await _context.SubMajorHeads
                //    .AnyAsync(s => s.SubMajorAccCode == request.dto.SubMajorAccCode, ct);

                //if (!subMajorHeadExists)
                //{
                //    _logger.LogWarning("Sub-major head with code {SubMajorAccCode} does not exist", request.dto.SubMajorAccCode);
                //    return new Response
                //    {
                //        IsSucced = false,
                //        Message = $"Sub-major head with code '{request.dto.SubMajorAccCode}' does not exist"
                //    };
                //}

                //// Verify that the sub-major head belongs to the specified major head
                //var subMajorHead = await _context.SubMajorHeads
                //    .FirstOrDefaultAsync(s => s.SubMajorAccCode == request.dto.SubMajorAccCode && 
                //                             s.MajorAccCode == request.dto.MajorAccCode, ct);

                //if (subMajorHead == null)
                //{
                //    _logger.LogWarning("Sub-major head {SubMajorAccCode} does not belong to major head {MajorAccCode}", 
                //        request.dto.SubMajorAccCode, request.dto.MajorAccCode);
                //    return new Response
                //    {
                //        IsSucced = false,
                //        Message = $"Sub-major head '{request.dto.SubMajorAccCode}' does not belong to major head '{request.dto.MajorAccCode}'"
                //    };
                //}

                // Map and create
                var minorHead = request.dto.Adapt<MinorHead>();
                minorHead.ModificationDate = DateTime.UtcNow;

                _context.MinorHeads.Add(minorHead);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Minor head {MinorAccCode} created successfully", request.dto.MinorAccCode);

                return new Response
                {
                    IsSucced = true,
                    Message = "Minor head created successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating minor head {MinorAccCode}", request.dto.MinorAccCode);
                
                // Check for specific database errors
                if (ex.InnerException?.Message.Contains("foreign key constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Foreign key constraint violation. Please ensure the major head and sub-major head exist and are properly related."
                    };
                }
                
                throw; // Re-throw for global handler
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating minor head {MinorAccCode}", request.dto.MinorAccCode);
                throw; // Re-throw for global handler
            }
        }
    }
}

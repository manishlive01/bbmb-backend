using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update
{
    public class UpdateMinorHeadHandler : IRequestHandler<UpdateMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<UpdateMinorHeadHandler> _logger;

        public UpdateMinorHeadHandler(BudgetContext context, ILogger<UpdateMinorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(UpdateMinorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Updating minor head with code: {MinorAccCode}", request.MinorAccCode);

                var minorHead = await _context.MinorHeads
                    .FirstOrDefaultAsync(m => m.MinorAccCode == request.MinorAccCode, ct);

                if (minorHead == null)
                {
                    _logger.LogWarning("Minor head with code {MinorAccCode} not found", request.MinorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor head with code '{request.MinorAccCode}' not found"
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

                // Update fields
                minorHead.MinorAccDesc = request.dto.MinorAccDesc;
                //minorHead.MajorAccCode = request.dto.MajorAccCode;
                //minorHead.SubMajorAccCode = request.dto.SubMajorAccCode;
                minorHead.UserCode = request.dto.UserCode;
                minorHead.ModificationDate = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Minor head {MinorAccCode} updated successfully", request.MinorAccCode);

                return new Response
                {
                    IsSucced = true,
                    Message = "Minor head updated successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating minor head {MinorAccCode}", request.MinorAccCode);
                
                if (ex.InnerException?.Message.Contains("foreign key constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Foreign key constraint violation. Please ensure the major head and sub-major head exist and are properly related."
                    };
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating minor head {MinorAccCode}", request.MinorAccCode);
                throw;
            }
        }
    }
}

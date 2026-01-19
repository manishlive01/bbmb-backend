using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Update
{
    public class UpdateSubMinorHeadHandler : IRequestHandler<UpdateSubMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<UpdateSubMinorHeadHandler> _logger;

        public UpdateSubMinorHeadHandler(BudgetContext context, ILogger<UpdateSubMinorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(UpdateSubMinorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Updating sub-minor head with code: {SubMinorAccCode}", request.SubMinorAccCode);

                var subMinorHead = await _context.SubMinorHeads
                    .FirstOrDefaultAsync(s => s.SubMinorAccCode == request.SubMinorAccCode, ct);

                if (subMinorHead == null)
                {
                    _logger.LogWarning("Sub-minor head with code {SubMinorAccCode} not found", request.SubMinorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Sub-minor head with code '{request.SubMinorAccCode}' not found"
                    };
                }

                // Check if major head exists
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

                // Check if minor head exists
                var minorHeadExists = await _context.MinorHeads
                    .AnyAsync(m => m.MinorAccCode == request.dto.MinorAccCode, ct);

                if (!minorHeadExists)
                {
                    _logger.LogWarning("Minor head with code {MinorAccCode} does not exist", request.dto.MinorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor head with code '{request.dto.MinorAccCode}' does not exist"
                    };
                }

                //// Verify that the minor head belongs to the specified sub-major head and major head
                //var minorHead = await _context.MinorHeads
                //    .FirstOrDefaultAsync(m => m.MinorAccCode == request.dto.MinorAccCode && 
                //                             m.SubMajorAccCode == request.dto.SubMajorAccCode &&
                //                             m.MajorAccCode == request.dto.MajorAccCode, ct);

                //if (minorHead == null)
                //{
                //    _logger.LogWarning("Minor head {MinorAccCode} does not belong to sub-major head {SubMajorAccCode} and major head {MajorAccCode}", 
                //        request.dto.MinorAccCode, request.dto.SubMajorAccCode, request.dto.MajorAccCode);
                //    return new Response
                //    {
                //        IsSucced = false,
                //        Message = $"Minor head '{request.dto.MinorAccCode}' does not belong to the specified sub-major head and major head"
                //    };
                //}

                // Update fields
                subMinorHead.SubMinorAccDesc = request.dto.SubMinorAccDesc;
                //subMinorHead.MajorAccCode = request.dto.MajorAccCode;
                //subMinorHead.SubMajorAccCode = request.dto.SubMajorAccCode;
                subMinorHead.MinorAccCode = request.dto.MinorAccCode;
                subMinorHead.UserCode = request.dto.UserCode;
                subMinorHead.ModificationDate = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Sub-minor head {SubMinorAccCode} updated successfully", request.SubMinorAccCode);

                return new Response
                {
                    IsSucced = true,
                    Message = "Sub-minor head updated successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating sub-minor head {SubMinorAccCode}", request.SubMinorAccCode);
                
                if (ex.InnerException?.Message.Contains("foreign key constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Foreign key constraint violation. Please ensure all parent heads exist and are properly related."
                    };
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating sub-minor head {SubMinorAccCode}", request.SubMinorAccCode);
                throw;
            }
        }
    }
}

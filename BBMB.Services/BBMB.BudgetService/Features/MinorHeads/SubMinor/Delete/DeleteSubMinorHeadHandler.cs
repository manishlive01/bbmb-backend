using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.Delete
{
    public class DeleteSubMinorHeadHandler : IRequestHandler<DeleteSubMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<DeleteSubMinorHeadHandler> _logger;

        public DeleteSubMinorHeadHandler(BudgetContext context, ILogger<DeleteSubMinorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(DeleteSubMinorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Deleting sub-minor head with code: {SubMinorAccCode}", request.SubMinorAccCode);

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

                _context.SubMinorHeads.Remove(subMinorHead);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Sub-minor head {SubMinorAccCode} deleted successfully", request.SubMinorAccCode);

                return new Response
                {
                    IsSucced = true,
                    Message = "Sub-minor head deleted successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting sub-minor head {SubMinorAccCode}", request.SubMinorAccCode);
                
                if (ex.InnerException?.Message.Contains("foreign key constraint") == true ||
                    ex.InnerException?.Message.Contains("FOREIGN KEY constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Cannot delete sub-minor head because it has dependent detail heads"
                    };
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting sub-minor head {SubMinorAccCode}", request.SubMinorAccCode);
                throw;
            }
        }
    }
}

using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.MinorHeads.Delete
{
    public class DeleteMinorHeadHandler : IRequestHandler<DeleteMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<DeleteMinorHeadHandler> _logger;

        public DeleteMinorHeadHandler(BudgetContext context, ILogger<DeleteMinorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(DeleteMinorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Deleting minor head with code: {MinorAccCode}", request.MinorAccCode);

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

                _context.MinorHeads.Remove(minorHead);

                try
                {
                    await _context.SaveChangesAsync(ct);
                    _logger.LogInformation("Minor head {MinorAccCode} deleted successfully", request.MinorAccCode);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "Cannot delete minor head {MinorAccCode} due to foreign key constraint", request.MinorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Cannot delete minor head. It has associated records (sub-minor heads or other dependencies)."
                    };
                }

                return new Response
                {
                    IsSucced = true,
                    Message = "Minor head deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting minor head {MinorAccCode}", request.MinorAccCode);
                throw;
            }
        }
    }
}

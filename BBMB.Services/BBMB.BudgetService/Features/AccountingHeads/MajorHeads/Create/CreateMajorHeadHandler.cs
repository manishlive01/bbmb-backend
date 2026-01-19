using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create
{
    public class CreateMajorHeadHandler
        : IRequestHandler<CreateMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<CreateMajorHeadHandler> _logger;

        public CreateMajorHeadHandler(BudgetContext context, ILogger<CreateMajorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(
            CreateMajorHeadCommand request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Creating major head with code: {MajorAccCode}", request.dto.MajorAccCode);

                // Check for duplicate
                var exists = await _context.MajorHeads
                    .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode, ct);

                if (exists)
                {
                    _logger.LogWarning("Major head with code {MajorAccCode} already exists", request.dto.MajorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major head with code '{request.dto.MajorAccCode}' already exists"
                    };
                }

                // Map DTO → Entity
                var majorHead = request.dto.Adapt<MajorHead>();
                majorHead.ModificationDate = DateTime.UtcNow;

                // Persist
                _context.MajorHeads.Add(majorHead);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Major head {MajorAccCode} created successfully", request.dto.MajorAccCode);

                return new Response
                {
                    IsSucced = true,
                    Message = "Major head created successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating major head {MajorAccCode}", request.dto.MajorAccCode);
                
                if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                    ex.InnerException?.Message.Contains("unique constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major head with code '{request.dto.MajorAccCode}' already exists"
                    };
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating major head {MajorAccCode}", request.dto.MajorAccCode);
                throw;
            }
        }
    }
}

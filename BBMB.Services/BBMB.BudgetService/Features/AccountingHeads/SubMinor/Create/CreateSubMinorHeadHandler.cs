using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Create
{
    public class CreateSubMinorHeadHandler
        : IRequestHandler<CreateSubMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<CreateSubMinorHeadHandler> _logger;

        public CreateSubMinorHeadHandler(
            BudgetContext context,
            ILogger<CreateSubMinorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(CreateSubMinorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation(
                    "Creating sub-minor head {SubMinorAccCode} for minor head {MinorAccCode}",
                    request.dto.SubMinorAccCode,
                    request.dto.MinorAccCode);

                // 1. Check Minor Head Exists
                bool minorExists = await _context.MinorHeads
                    .AnyAsync(x => x.MinorAccCode == request.dto.MinorAccCode, ct);

                if (!minorExists)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor head '{request.dto.MinorAccCode}' not found"
                    };
                }

                // 2. Check Duplicate ONLY within same Minor Head
                bool exists = await _context.SubMinorHeads
                    .AnyAsync(x =>
                        x.SubMinorAccCode == request.dto.SubMinorAccCode &&
                        x.MinorAccCode == request.dto.MinorAccCode,
                        ct);

                if (exists)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message =
                            $"Sub-minor '{request.dto.SubMinorAccCode}' already exists under Minor '{request.dto.MinorAccCode}'"
                    };
                }

                // 3. Map DTO
                var entity = request.dto.Adapt<SubMinorHead>();
                entity.ModificationDate = DateTime.UtcNow;

                // 4. Save
                _context.SubMinorHeads.Add(entity);
                await _context.SaveChangesAsync(ct);

                return new Response
                {
                    IsSucced = true,
                    Message = "Sub-minor head created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SubMinorHead");
                return new Response
                {
                    IsSucced = false,
                    Message = "Internal server error"
                };
            }
        }
    }
}

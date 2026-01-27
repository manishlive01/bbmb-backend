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
                    "Creating Sub-Minor Head {SubMinorAccCode} under Minor {MinorAccCode}",
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
                        Message = $"Minor head '{request.dto.MinorAccCode}' does not exist"
                    };
                }

                // 2. Check duplicate ONLY within same Minor Head
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

                // 3. Map DTO → Entity
                var entity = request.dto.Adapt<SubMinorHead>();
                entity.ModificationDate = DateTime.UtcNow;

                // 4. Save
                _context.SubMinorHeads.Add(entity);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Sub-Minor Head {SubMinorAccCode} created under Minor {MinorAccCode}",
                    request.dto.SubMinorAccCode,
                    request.dto.MinorAccCode);

                return new Response
                {
                    IsSucced = true,
                    Message = "Sub-minor head created successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Database error while creating Sub-Minor Head {SubMinorAccCode}",
                    request.dto.SubMinorAccCode);

                if (ex.InnerException?.Message.Contains("foreign key") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Invalid Minor Head reference"
                    };
                }

                if (ex.InnerException?.Message.Contains("duplicate") == true ||
                    ex.InnerException?.Message.Contains("unique") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Sub-minor head already exists for this minor head"
                    };
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while creating Sub-Minor Head {SubMinorAccCode}",
                    request.dto.SubMinorAccCode);
                throw;
            }
        }
    }
}

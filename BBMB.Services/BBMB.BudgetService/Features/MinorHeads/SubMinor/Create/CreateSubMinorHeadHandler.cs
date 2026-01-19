using BBMB.BudgetService.Features.MinorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.Create
{
    public class CreateSubMinorHeadHandler : IRequestHandler<CreateSubMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<CreateSubMinorHeadHandler> _logger;

        public CreateSubMinorHeadHandler(BudgetContext context, ILogger<CreateSubMinorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(CreateSubMinorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Creating sub-minor head with code: {SubMinorAccCode}", request.dto.SubMinorAccCode);

                // Check for duplicate
                var exists = await _context.SubMinorHeads
                    .AnyAsync(s => s.SubMinorAccCode == request.dto.SubMinorAccCode, ct);

                if (exists)
                {
                    _logger.LogWarning("Sub-minor head with code {SubMinorAccCode} already exists", request.dto.SubMinorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Sub-minor head with code '{request.dto.SubMinorAccCode}' already exists"
                    };
                }

                // Check if major head exists
                var majorHeadExists = await _context.MajorHeads
                    .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode, ct);

                if (!majorHeadExists)
                {
                    _logger.LogWarning("Major head with code {MajorAccCode} does not exist", request.dto.MajorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major head with code '{request.dto.MajorAccCode}' does not exist"
                    };
                }

                // Check if sub-major head exists
                var subMajorHeadExists = await _context.SubMajorHeads
                    .AnyAsync(s => s.SubMajorAccCode == request.dto.SubMajorAccCode, ct);

                if (!subMajorHeadExists)
                {
                    _logger.LogWarning("Sub-major head with code {SubMajorAccCode} does not exist", request.dto.SubMajorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Sub-major head with code '{request.dto.SubMajorAccCode}' does not exist"
                    };
                }

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

                // Verify that the minor head belongs to the specified sub-major head and major head
                var minorHead = await _context.MinorHeads
                    .FirstOrDefaultAsync(m => m.MinorAccCode == request.dto.MinorAccCode && 
                                             m.SubMajorAccCode == request.dto.SubMajorAccCode &&
                                             m.MajorAccCode == request.dto.MajorAccCode, ct);

                if (minorHead == null)
                {
                    _logger.LogWarning("Minor head {MinorAccCode} does not belong to sub-major head {SubMajorAccCode} and major head {MajorAccCode}", 
                        request.dto.MinorAccCode, request.dto.SubMajorAccCode, request.dto.MajorAccCode);
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor head '{request.dto.MinorAccCode}' does not belong to the specified sub-major head and major head"
                    };
                }

                // Map and create
                var subMinorHead = request.dto.Adapt<SubMinorHead>();
                subMinorHead.ModificationDate = DateTime.UtcNow;

                _context.SubMinorHeads.Add(subMinorHead);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Sub-minor head {SubMinorAccCode} created successfully", request.dto.SubMinorAccCode);

                return new Response
                {
                    IsSucced = true,
                    Message = "Sub-minor head created successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating sub-minor head {SubMinorAccCode}", request.dto.SubMinorAccCode);
                
                // Check for specific database errors
                if (ex.InnerException?.Message.Contains("foreign key constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Foreign key constraint violation. Please ensure all parent heads exist and are properly related."
                    };
                }
                
                throw; // Re-throw for global handler
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating sub-minor head {SubMinorAccCode}", request.dto.SubMinorAccCode);
                throw; // Re-throw for global handler
            }
        }
    }
}

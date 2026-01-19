using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public class CreateSubMajorHeadHandler : IRequestHandler<CreateSubMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly ILogger<CreateSubMajorHeadHandler> _logger;

        public CreateSubMajorHeadHandler(BudgetContext context, ILogger<CreateSubMajorHeadHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> Handle(CreateSubMajorHeadCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Creating sub-major head with code: {SubMajorAccCode} for major head: {MajorAccCode}", 
                    request.dto.SubMajorAccCode, request.dto.MajorAccCode);

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

                // Check for duplicate
                var exists = await _context.SubMajorHeads
                    .AnyAsync(s => s.SubMajorAccCode == request.dto.SubMajorAccCode && 
                                  s.MajorAccCode == request.dto.MajorAccCode, ct);
                    
                if (exists)
                {
                    _logger.LogWarning("Sub-major head with code {SubMajorAccCode} already exists for major head {MajorAccCode}", 
                        request.dto.SubMajorAccCode, request.dto.MajorAccCode);
                    return new Response 
                    { 
                        IsSucced = false, 
                        Message = $"Sub-major head with code '{request.dto.SubMajorAccCode}' already exists for major head '{request.dto.MajorAccCode}'" 
                    };
                }
                
                // Map and create
                var subMajorHead = request.dto.Adapt<SubMajorHead>();
                subMajorHead.ModificationDate = DateTime.UtcNow;
                
                _context.SubMajorHeads.Add(subMajorHead);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Sub-major head {SubMajorAccCode} created successfully for major head {MajorAccCode}", 
                    request.dto.SubMajorAccCode, request.dto.MajorAccCode);
                
                return new Response 
                { 
                    IsSucced = true, 
                    Message = "Sub-major head created successfully" 
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating sub-major head {SubMajorAccCode}", request.dto.SubMajorAccCode);
                
                if (ex.InnerException?.Message.Contains("foreign key constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major head with code '{request.dto.MajorAccCode}' does not exist"
                    };
                }
                
                if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                    ex.InnerException?.Message.Contains("unique constraint") == true)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Sub-major head with code '{request.dto.SubMajorAccCode}' already exists"
                    };
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating sub-major head {SubMajorAccCode}", request.dto.SubMajorAccCode);
                throw;
            }
        }
    }
}

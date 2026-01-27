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
                _logger.LogInformation("Creating sub-major head {SubMajorAccCode} for major head {MajorAccCode}",
                    request.dto.SubMajorAccCode, request.dto.MajorAccCode);

                // 1. Check Major Head Exists
                bool majorExists = await _context.MajorHeads
                    .AnyAsync(x => x.MajorAccCode == request.dto.MajorAccCode, ct);

                if (!majorExists)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major head '{request.dto.MajorAccCode}' not found"
                    };
                }

                // 2. Check Duplicate ONLY within same Major Head
                bool exists = await _context.SubMajorHeads
                    .AnyAsync(x => x.SubMajorAccCode == request.dto.SubMajorAccCode
                                && x.MajorAccCode == request.dto.MajorAccCode, ct);

                if (exists)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Sub-major '{request.dto.SubMajorAccCode}' already exists under Major '{request.dto.MajorAccCode}'"
                    };
                }

                // 3. Map DTO
                var entity = request.dto.Adapt<SubMajorHead>();
                entity.ModificationDate = DateTime.UtcNow;

                // 4. Save
                _context.SubMajorHeads.Add(entity);
                await _context.SaveChangesAsync(ct);

                return new Response
                {
                    IsSucced = true,
                    Message = "Sub-major head created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SubMajorHead");
                return new Response
                {
                    IsSucced = false,
                    Message = "Internal server error"
                };
            }
        }

    }
}

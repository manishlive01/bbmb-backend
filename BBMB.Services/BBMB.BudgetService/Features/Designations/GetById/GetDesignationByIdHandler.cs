using BBMB.BudgetService.Features.Designations.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.GetById
{
    public class GetDesignationByIdHandler
        : IRequestHandler<GetDesignationByIdQuery, GetDesignationByIdResponseDto?>
    {
        private readonly BudgetContext _context;

        public GetDesignationByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetDesignationByIdResponseDto?> Handle(
            GetDesignationByIdQuery request,
            CancellationToken ct)
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(d => d.DesignationCode == request.Request.Id)
                .Select(d => new GetDesignationByIdResponseDto
                {
                    DesignationCode = d.DesignationCode,
                    DesignationDesc = d.DesignationDesc,
                    ClassCode = d.ClassCode,
                    MinBasic = d.MinBasic,
                    MaxBasic = d.MaxBasic,
                    ModificationDate = d.ModificationDate,
                    UserCode = d.UserCode
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}

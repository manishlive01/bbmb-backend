using BBMB.BudgetService.Features.Geographical.States.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Geographical.States.GetList
{
    public class GetStateMasterListHandler
        : IRequestHandler<GetStateMasterListCommand, List<GetStateMasterListResponseDto>>
    {
        private readonly BBMBContext _context;

        public GetStateMasterListHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<List<GetStateMasterListResponseDto>> Handle(
            GetStateMasterListCommand request,
            CancellationToken ct)
        {
            return await _context.StateMasters
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.StateName)
                .Select(x => new GetStateMasterListResponseDto
                {
                    StateMasterId = x.StateMasterId,
                    StateCode = x.StateCode,
                    StateName = x.StateName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }
    }
}

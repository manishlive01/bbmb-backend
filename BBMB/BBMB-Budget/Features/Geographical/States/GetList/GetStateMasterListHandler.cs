using BBMB_Budget.Features.Geographical.States.Entities;
using BBMB_Budget.Features.Geographical.States.GetList.Dtos;
using BBMB_Budget.Infrastructure.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Geographical.States.GetList
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
            var states = await _context.StateMasters
                .AsNoTracking()
                .Where(x => x.IsDeleted == false)
                .OrderBy(x => x.StateName)
                .ToListAsync(ct);

            return states.Adapt<List<GetStateMasterListResponseDto>>();
        }
    }
}

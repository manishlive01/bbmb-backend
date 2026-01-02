using BBMB_Budget.Features.Geographical.States.GetById.Dtos;
using BBMB_Budget.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Geographical.States.GetById
{
    public class GetStateMasterByIdHandler
        : IRequestHandler<GetStateMasterByIdCommand, GetStateMasterByIdResponseDto?>
    {
        private readonly BBMBContext _context;

        public GetStateMasterByIdHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<GetStateMasterByIdResponseDto?> Handle(
            GetStateMasterByIdCommand request,
            CancellationToken ct)
        {
            var id = request.Request.Id;

            return await _context.StateMasters
                .AsNoTracking()
                .Where(x =>
                    x.StateMasterId == id &&
                    !x.IsDeleted)
                .Select(x => new GetStateMasterByIdResponseDto
                {
                    StateCode = x.StateCode,
                    StateName = x.StateName,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}

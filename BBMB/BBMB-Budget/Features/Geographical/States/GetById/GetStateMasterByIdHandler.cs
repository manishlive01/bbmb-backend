using BBMB_Budget.Features.Geographical.States.Entities;
using BBMB_Budget.Features.Geographical.States.GetById.Dtos;
using BBMB_Budget.Infrastructure.Persistence;
using Mapster;
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

            var state = await _context.StateMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.StateMasterId == id &&
                    x.IsDeleted == false,
                    ct);

            return state?.Adapt<GetStateMasterByIdResponseDto>();
        }
    }
}

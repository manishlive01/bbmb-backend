using BBMB_Budget.Infrastructure.Persistence;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Geographical.States.Delete
{
    public class DeleteStateMasterHandler
        : IRequestHandler<DeleteStateMasterCommand, Response>
    {
        private readonly BBMBContext _context;

        public DeleteStateMasterHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            DeleteStateMasterCommand request,
            CancellationToken ct)
        {
            var id = request.Request.Id;

            // 1️⃣ Fetch entity
            var state = await _context.StateMasters
                .FirstOrDefaultAsync(x =>
                    x.StateMasterId == id &&
                    x.IsDeleted == false,
                    ct);

            if (state == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "State not found"
                };
            }

            // 2️⃣ Soft delete
            state.IsDeleted = true;
            state.DeletedAt = DateTime.UtcNow;

            // Optional: deactivate
            state.IsActive = false;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "State deleted successfully"
            };
        }
    }
}

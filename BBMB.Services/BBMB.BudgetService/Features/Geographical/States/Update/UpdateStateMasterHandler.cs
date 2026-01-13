using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Geographical.States.Update
{
    public class UpdateStateMasterHandler
        : IRequestHandler<UpdateStateMasterCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateStateMasterHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateStateMasterCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Fetch existing state
            var state = await _context.StateMasters
                .FirstOrDefaultAsync(x =>
                    x.StateMasterId == dto.StateMasterId &&
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

            // 2️⃣ Check duplicate StateCode (excluding self)
            var duplicate = await _context.StateMasters
                .AnyAsync(x =>
                    x.StateCode == dto.StateCode &&
                    x.StateMasterId != dto.StateMasterId &&
                    x.IsDeleted == false,
                    ct);

            if (duplicate)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "State code already exists"
                };
            }

            // 3️⃣ Map updated fields
            dto.Adapt(state);

            // 4️⃣ System fields
            state.UpdatedAt = DateTime.UtcNow;

            // 5️⃣ Save
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "State updated successfully"
            };
        }
    }
}

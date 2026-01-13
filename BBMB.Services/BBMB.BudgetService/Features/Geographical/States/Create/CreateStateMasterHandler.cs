using BBMB.BudgetService.Features.Geographical.States.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Geographical.States.Create
{
    public class CreateStateMasterHandler
        : IRequestHandler<CreateStateMasterCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateStateMasterHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateStateMasterCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Map DTO → Entity
            var state = request.dto.Adapt<StateMaster>();

            // 2️⃣ Duplicate check
            var exists = await _context.StateMasters
                .AnyAsync(x =>
                    x.StateCode == state.StateCode &&
                    x.IsDeleted == false,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "State code already exists"
                };
            }

            // 3️⃣ System fields
            state.IsDeleted = false;
            state.CreatedAt = DateTime.UtcNow;

            // 4️⃣ Persist
            _context.StateMasters.Add(state);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "State created successfully"
            };
        }
    }
}

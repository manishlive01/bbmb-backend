using BBMB_Budget.Features.Budgets.BudgetTypes.Entities;
using BBMB_Budget.Features.Geographical.States.Create;
using BBMB_Budget.Features.Geographical.States.Entities;
using BBMB_Budget.Infrastructure.Persistence;
using BBMB_Budget.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Create
{
    public class CreateBudgetTypeMasterHandler
         : IRequestHandler<CreateBudgetTypeMasterCommand, Response>
    {
        private readonly BBMBContext _context;

        public CreateBudgetTypeMasterHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateBudgetTypeMasterCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Map DTO → Entity
            var state = request.dto.Adapt<BudgetTypeMaster>();

            // 2️⃣ Duplicate check
            var exists = await _context.BudgetTypeMasters
                .AnyAsync(x =>
                    x.BudgetTypeCode == state.BudgetTypeCode &&
                    x.IsDeleted == false,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Budget code already exists"
                };
            }

            // 3️⃣ System fields
            state.IsDeleted = false;
            state.CreatedAt = DateTime.UtcNow;

            // 4️⃣ Persist
            _context.BudgetTypeMasters.Add(state);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Budget Type created successfully"
            };
        }
    }
}

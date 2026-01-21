using BBMB.BudgetService.Features.Offices.AccountOffices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.Create
{
    public class CreateAccountOfficeHandler
        : IRequestHandler<CreateAccountOfficeCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateAccountOfficeHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateAccountOfficeCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Duplicate check
            var exists = await _context.AccountOffices
                .AnyAsync(x =>
                    x.AccOffice == request.dto.AccOffice,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Account Office with code '{request.dto.AccOffice}' already exists"
                };
            }

            // 2️⃣ Map DTO → Entity
            var accountOffice = request.dto.Adapt<AccountOffice>();

            // 3️⃣ System fields
            accountOffice.ModificationDate = DateTime.UtcNow;
            accountOffice.UserCode = "123"; // system-controlled

            // 4️⃣ Persist
            _context.AccountOffices.Add(accountOffice);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Account Office created successfully"
            };
        }
    }
}

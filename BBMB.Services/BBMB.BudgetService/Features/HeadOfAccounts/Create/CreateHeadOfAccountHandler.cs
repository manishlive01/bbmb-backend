using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    public class CreateHeadOfAccountHandler : IRequestHandler<CreateHeadOfAccountCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateHeadOfAccountHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateHeadOfAccountCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            bool exists = await _context.HeadOfAccounts.AnyAsync(x =>
                x.MajorAccCode == dto.MajorAccCode &&
                x.SubMajorAccCode == dto.SubMajorAccCode &&
                x.MinorAccCode == dto.MinorAccCode &&
                x.SubMinorAccCode == dto.SubMinorAccCode &&
                x.DetailAccCode == dto.DetailAccCode &&
                x.ObjectAccCode == dto.ObjectAccCode,
                ct
            );

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Head of account already exists"
                };
            }

            var headOfAccount = dto.Adapt<HeadOfAccount>();
            headOfAccount.ModificationDate = DateTime.UtcNow;

           await _context.HeadOfAccounts.AddAsync(headOfAccount);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Head of account created successfully"
            };
        }


    }
}

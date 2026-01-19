using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Divisions.Update
{
    public class UpdateDivisionHandler : IRequestHandler<UpdateDivisionCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateDivisionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateDivisionCommand request, CancellationToken ct)
        {
            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.DivisionCode == request.Code, ct);

            if (division == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with code '{request.Code}' not found"
                };
            }

            // Update fields
            division.DivSection = request.dto.DivSection;
            division.DivisionDesc = request.dto.DivisionDesc;
            division.CtrlOffice1 = request.dto.CtrlOffice1;
            division.CtrlOffice2 = request.dto.CtrlOffice2;
            division.RoCode = request.dto.RoCode;
            division.MajorAccCode = request.dto.MajorAccCode;
            division.AccOffice = request.dto.AccOffice;
            division.UnitCode = request.dto.UnitCode;
            division.PoolCode = request.dto.PoolCode;
            division.UmdUnit = request.dto.UmdUnit;
            division.RegionCode = request.dto.RegionCode;
            division.DdoTds = request.dto.DdoTds;
            division.ParentDivisionCode = request.dto.ParentDivisionCode;
            division.UserCode = request.dto.UserCode;
            division.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Division updated successfully"
            };
        }
    }
}

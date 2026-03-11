using BBMB.BudgetService.Features.PublicMasters.AppointmentTypes.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PublicMasters.AppointmentTypes
{
    public class GetAppointmentTypeListHandler
     : IRequestHandler<GetAppointmentTypeListCommand, List<GetAppointmentTypeListResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetAppointmentTypeListHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetAppointmentTypeListResponseDto>> Handle(
            GetAppointmentTypeListCommand request,
            CancellationToken ct)
        {
            return await _context.AppointmentTypes
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsStatus)
                .OrderBy(x => x.AppointmentTypeName)
                .Select(x => new GetAppointmentTypeListResponseDto
                {
                    Id = x.Id,
                    AppointmentTypeName = x.AppointmentTypeName
                })
                .ToListAsync(ct);
        }
    }
}

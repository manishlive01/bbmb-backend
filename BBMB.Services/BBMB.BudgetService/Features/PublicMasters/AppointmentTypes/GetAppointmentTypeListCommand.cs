using BBMB.BudgetService.Features.PublicMasters.AppointmentTypes.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.AppointmentTypes
{
    public sealed record GetAppointmentTypeListCommand()
    : IRequest<List<GetAppointmentTypeListResponseDto>>;
}

using BBMB.BudgetService.Features.BudgetProcess.Calendars.Get.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Get
{
    public sealed record GetCalendarQuery(long CycleId) : IRequest<Response<CalendarViewDto>>;
}

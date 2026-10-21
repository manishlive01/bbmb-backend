using BBMB.BudgetService.Features.BudgetProcess.Calendars.Set.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Set
{
    public sealed record SetCalendarCommand(long CycleId, SetCalendarDto Dto) : IRequest<Response>;
}

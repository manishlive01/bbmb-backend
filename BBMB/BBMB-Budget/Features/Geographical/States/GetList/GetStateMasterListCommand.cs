using BBMB_Budget.Features.Geographical.States.Create.Dtos;
using BBMB_Budget.Features.Geographical.States.GetList.Dtos;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.States.GetList
{
    public sealed record GetStateMasterListCommand() : IRequest<List<GetStateMasterListResponseDto>>;
}

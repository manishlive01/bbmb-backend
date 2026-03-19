using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetById
{
    public class GetCEManagementByIdHandler
        : IRequestHandler<GetCEManagementByIdQuery, Response<GetCEManagementByIdResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetCEManagementByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response<GetCEManagementByIdResponseDto>> Handle(
            GetCEManagementByIdQuery request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.CEManagements
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CECode == request.ceCode, cancellationToken);

            if (entity == null)
            {
                return new Response<GetCEManagementByIdResponseDto>
                {
                    IsSucced = false,
                    Message = "CE Management record not found"
                };
            }

            var dto = entity.Adapt<GetCEManagementByIdResponseDto>();

            return new Response<GetCEManagementByIdResponseDto>
            {
                IsSucced = true,
                Data = dto
            };
        }
    }
}
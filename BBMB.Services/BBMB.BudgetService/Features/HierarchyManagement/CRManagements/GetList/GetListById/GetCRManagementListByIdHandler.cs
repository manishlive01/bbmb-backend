using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.GetList;
using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.GetList.GetListById
{
    public class GetCRManagementListByIdHandler
       : IRequestHandler<GetCRManagementListById, List<GetCRManagementListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetCRManagementListByIdHandler(BudgetContext context)
        {
            _context = context;
        }


        public async Task<List<GetCRManagementListResponseDto>> Handle(
      GetCRManagementListById request,
      CancellationToken cancellationToken)
        {
            return await _context.CRManagements.Include(d=>d.CEManagement)
                .AsNoTracking()
                .Where(x => x.CEId == request.RequestDto.Id)
                .Select(x => new GetCRManagementListResponseDto
                {
                    CRCode = x.CRCode,
                    CECode = x.CEManagement.CECode,
                    Name = x.Name,
                    Description = x.Description,
                    Location = x.Location,
                    Date = x.Date,
                    Status = x.Status,
                    UserCode = x.UserCode
                })
                .ToListAsync(cancellationToken);
        }


    }
}

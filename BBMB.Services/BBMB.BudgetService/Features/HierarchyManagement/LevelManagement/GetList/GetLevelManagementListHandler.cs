using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.GetList;
using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Dapper;
using MediatR;
using System.Data;
public class GetLevelManagementListHandler
    : IRequestHandler<GetLevelManagementListQuery, PagedResponse<GetLevelListResponseDto>>
{
    private readonly IDbConnection _connection;

    public GetLevelManagementListHandler(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<PagedResponse<GetLevelListResponseDto>> Handle(
    GetLevelManagementListQuery request,
    CancellationToken cancellationToken)
    { 


        var query = @"
        SELECT 
            id,
            name AS Name, 
            status AS Status
        FROM budget.levels
        ORDER BY Name;
    ";

        var data = await _connection.QueryAsync<GetLevelListResponseDto>(query);

        return new PagedResponse<GetLevelListResponseDto>
        {
            Message = "Records retrieved successfully",
            Data = data.ToList()  
        };
    }
}
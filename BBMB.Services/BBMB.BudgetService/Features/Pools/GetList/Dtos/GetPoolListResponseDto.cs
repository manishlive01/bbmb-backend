namespace BBMB.BudgetService.Features.Pools.GetList.Dtos
{
    public class GetPoolListResponseDto
    {
        public string PoolCode { get; set; } = string.Empty;
        public string PoolDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}

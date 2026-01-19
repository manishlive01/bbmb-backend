namespace BBMB.BudgetService.Features.Pools.GetById.Dtos
{
    public class GetPoolByIdResponseDto
    {
        public string PoolCode { get; set; } = string.Empty;
        public string PoolDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}

namespace BBMB.BudgetService.Features.Pools.Create.Dtos
{
    public class CreatePoolDto
    {
        public string PoolCode { get; set; } = string.Empty;
        public string PoolDesc { get; set; } = string.Empty;
        public string UserCode { get; set; } = "123";
    }
}

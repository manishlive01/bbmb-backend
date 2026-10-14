namespace BBMB.BudgetService.Tests.Shared;

/// <summary>Tests in this collection share one migrated postgres:17 container (PD-3).</summary>
[CollectionDefinition(Name)]
public sealed class BudgetDatabaseCollection : ICollectionFixture<BudgetDatabaseFixture>
{
    public const string Name = "BudgetDatabase";
}

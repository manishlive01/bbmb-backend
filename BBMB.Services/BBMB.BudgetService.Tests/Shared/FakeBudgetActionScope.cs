using BBMB.BudgetService.Shared.Services.BudgetActors;
using Microsoft.EntityFrameworkCore.Storage;
namespace BBMB.BudgetService.Tests.Shared;
/// <summary>
/// In-memory stand-in for <see cref="IBudgetActionScope"/>: the real scope issues <c>set_config</c> SQL and a
/// PostgreSQL transaction, neither of which the EF in-memory provider supports. Records every action code opened,
/// so a test can assert a write went through the audited scope. The transaction is a no-op.
/// </summary>
public sealed class FakeBudgetActionScope : IBudgetActionScope
{
    public List<string> ActionCodes { get; } = [];
    public int Commits { get; private set; }

    public Task<IDbContextTransaction> BeginAsync(
        string actionCode, string? justification = null, bool noJustification = false,
        string? onBehalfOfPost = null, CancellationToken ct = default)
    {
        ActionCodes.Add(actionCode);
        return Task.FromResult<IDbContextTransaction>(new NoOpTransaction(this));
    }

    private sealed class NoOpTransaction(FakeBudgetActionScope owner) : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();
        public void Commit() => owner.Commits++;
        public Task CommitAsync(CancellationToken cancellationToken = default) { owner.Commits++; return Task.CompletedTask; }
        public void Rollback() { }
        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

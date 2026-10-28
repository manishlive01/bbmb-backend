using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BBMB.BudgetService.Tests.Common
{
    public abstract class InMemoryTestBase : IDisposable
    {
        protected BudgetContext Context { get; }

        protected InMemoryTestBase()
        {
            var options = new DbContextOptionsBuilder<BudgetContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new BudgetContext(options);
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}

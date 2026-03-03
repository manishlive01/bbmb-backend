using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.FinanceContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBMB.BudgetService.Infrastructure.DependencyInjection
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabaseContexts(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            // ===============================
            // Budget Schema
            // ===============================
            services.AddDbContextPool<BudgetContext>(options =>
                options.UseNpgsql(connectionString),
                poolSize: 5);

            // ===============================
            // Finance Schema
            // ===============================
            services.AddDbContextPool<FinanceContext>(options =>
                options.UseNpgsql(connectionString),
                poolSize: 5);

            // ===============================
            // Employee Schema
            // ===============================
            services.AddDbContextPool<EmployeeContext>(options =>
                options.UseNpgsql(connectionString),
                poolSize: 5);

            return services;
        }
    }
}
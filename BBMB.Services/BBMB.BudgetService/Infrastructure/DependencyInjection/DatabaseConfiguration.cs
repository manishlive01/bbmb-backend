using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Infrastructure.Persistence.FinanceContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

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
                poolSize: 2);
            // ===============================
            //  Dapper context 
            // ===============================

            services.AddScoped<IDbConnection>(sp =>
        new NpgsqlConnection(
            sp.GetRequiredService<IConfiguration>()
              .GetConnectionString("DefaultConnection")));
            // ===============================
            // Finance Schema
            // ===============================
            services.AddDbContextPool<FinanceContext>(options =>
                options.UseNpgsql(connectionString),
                poolSize: 2);

            // ===============================
            // Employee Schema
            // ===============================
            services.AddDbContextPool<EmployeeContext>(options =>
                options.UseNpgsql(connectionString),
                poolSize: 3);

            return services;
        }
    }
}
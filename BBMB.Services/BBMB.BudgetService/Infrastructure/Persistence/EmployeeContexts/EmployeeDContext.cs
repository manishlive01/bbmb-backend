using Npgsql;
using System.Data;

namespace BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts
{
    /// <summary>
    /// EmployeeDContext
    /// ----------------
    /// Central place to create and manage database connections for Dapper
    /// related to Employee module.
    ///
    /// Purpose:
    /// - Centralizes Employee DB connection logic
    /// - Avoids hard-coded connection strings
    /// - Enables clean separation between Budget, Finance & Employee databases
    /// - Works alongside EF Core if required
    ///
    /// Lifetime:
    /// - Registered as Scoped (1 instance per HTTP request)
    /// </summary>
    public class EmployeeDContext
    {
        // Holds application configuration (appsettings.json, environment variables, etc.)
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor
        /// - IConfiguration is injected via Dependency Injection
        /// - Used to read Employee database connection string
        /// </summary>
        /// <param name="config">Application configuration</param>
        public EmployeeDContext(IConfiguration config)
        {
            _configuration = config;
        }

        /// <summary>
        /// Creates and returns a NEW database connection for Employee Dapper operations.
        ///
        /// Notes:
        /// - Uses Npgsql (PostgreSQL provider)
        /// - Connection string key: "EmployeeConnection"
        /// - Caller is responsible for opening and disposing the connection
        ///
        /// Typical Usage:
        /// using var connection = _employeeDContext.CreateConnection();
        /// var data = await connection.QueryAsync(...);
        /// </summary>
        /// <returns>IDbConnection (PostgreSQL)</returns>
        public IDbConnection CreateConnection()
            => new NpgsqlConnection(
                _configuration.GetConnectionString("EmployeeConnection"));
    }
}
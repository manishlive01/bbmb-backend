using Npgsql;
using System.Data;

namespace BBMB_Budget.Infrastructure.Persistence
{
    /// <summary>
    /// DapperContext
    /// ----------------
    /// Central place to create and manage database connections for Dapper.
    /// 
    /// Purpose:
    /// - Keeps connection string logic in one place
    /// - Avoids hard-coding connection strings
    /// - Allows clean usage of Dapper alongside EF Core
    /// 
    /// Lifetime:
    /// - Registered as Scoped (1 instance per HTTP request)
    /// </summary>
    public class DapperContext
    {
        // Holds application configuration (appsettings.json, env vars, etc.)
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor
        /// - IConfiguration is injected by Dependency Injection
        /// - Used to read the database connection string
        /// </summary>
        /// <param name="config">Application configuration</param>
        public DapperContext(IConfiguration config)
        {
            _configuration = config;
        }

        /// <summary>
        /// Creates and returns a NEW database connection for Dapper.
        /// 
        /// Notes:
        /// - Uses Npgsql (PostgreSQL provider)
        /// - Connection string key: "DefaultConnection"
        /// - Caller is responsible for opening/disposing the connection
        /// 
        /// Typical Usage:
        /// using var connection = _dapperContext.CreateConnection();
        /// var data = await connection.QueryAsync(...);
        /// </summary>
        /// <returns>IDbConnection (PostgreSQL)</returns>
        public IDbConnection CreateConnection()
            => new NpgsqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));
    }
}

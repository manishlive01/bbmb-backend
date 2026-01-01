namespace BBMB_Budget.Shared.Endpoints
{
    /// <summary>
    /// IEndpoint
    /// ---------
    /// This interface is a contract for all API endpoint classes.
    /// 
    /// In simple words:
    /// - Any class that represents API routes MUST implement this interface
    /// - It forces every endpoint class to define how its routes are mapped
    /// 
    /// Why this is needed:
    /// - Allows automatic endpoint registration using reflection
    /// - Keeps endpoint code organized and modular
    /// - Prevents putting all endpoints inside Program.cs
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// This method is responsible for registering API routes.
        /// 
        /// Layman explanation:
        /// - This is where you write app.MapGet(), app.MapPost(), etc.
        /// - ASP.NET Core calls this method during app startup
        /// 
        /// Example:
        /// public void Map(IEndpointRouteBuilder app)
        /// {
        ///     app.MapGet("/users", () => "Hello Users");
        /// }
        /// </summary>
        /// <param name="app">ASP.NET Core route builder</param>
        void Map(IEndpointRouteBuilder app);
    }
}

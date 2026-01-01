using System.Reflection;

namespace BBMB_Budget.Shared.Endpoints.EndpointExtensions
{
    /// <summary>
    /// EndpointExtensions
    /// ------------------
    /// This class automatically finds and registers all API endpoints.
    /// 
    /// Why this exists:
    /// - Keeps Program.cs clean
    /// - Avoids manually calling each endpoint class
    /// - Supports modular, scalable Minimal API design
    /// 
    /// How it works (in simple words):
    /// - It looks at the current assembly (project)
    /// - Finds all classes that implement IEndpoint
    /// - Creates an object of each endpoint
    /// - Calls its Map() method to register routes
    /// </summary>
    public static class EndpointExtensions
    {
        /// <summary>
        /// This method is called from Program.cs like:
        /// app.MapEndpoints();
        /// 
        /// It automatically registers all endpoints
        /// that implement the IEndpoint interface.
        /// </summary>
        /// <param name="app">ASP.NET Core route builder</param>
        public static void MapEndpoints(this IEndpointRouteBuilder app)
        {
            // 🔍 Step 1:
            // Get all types (classes) from the current project assembly
            var endpoints = Assembly.GetExecutingAssembly()
                .DefinedTypes

                // 🔍 Step 2:
                // Filter only those classes which:
                // - Implement IEndpoint
                // - Are NOT abstract classes
                // - Are NOT interfaces
                .Where(t =>
                    typeof(IEndpoint).IsAssignableFrom(t) &&
                    t is { IsAbstract: false, IsInterface: false });

            // 🔁 Step 3:
            // Loop through each endpoint class found
            foreach (var endpoint in endpoints)
            {
                // 🏗 Step 4:
                // Create an instance of the endpoint class
                // (This requires a parameterless constructor)
                var instance = (IEndpoint)Activator.CreateInstance(endpoint)!;

                // 🚦 Step 5:
                // Call the Map method to register routes
                // Example: app.MapGet(), app.MapPost(), etc.
                instance.Map(app);
            }
        }
    }
}

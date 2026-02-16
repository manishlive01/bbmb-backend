using Auth.Authorization.Handlers;
using Auth.Authorization.PolicyProvider;
using Microsoft.AspNetCore.Authorization;

namespace BBMB.BudgetService.Authorization.Extension
{
    public static class AuthorizationExtension
    {
        public static IServiceCollection AddProjectRoleAuthorization(
            this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, ProjectRoleHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, ProjectRolePolicyProvider>();

            return services;
        }
    }
}

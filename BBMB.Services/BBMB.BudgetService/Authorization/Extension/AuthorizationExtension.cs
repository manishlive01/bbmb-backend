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
            // BudgetRight:<key> policies (design §4.4).
            services.AddSingleton<IAuthorizationHandler, BudgetRightHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, ProjectRolePolicyProvider>();

            return services;
        }
    }
}

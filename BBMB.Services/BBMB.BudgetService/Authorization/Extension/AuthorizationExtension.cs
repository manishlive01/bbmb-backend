using BBMB.BudgetService.Authorization.Handlers;
using BBMB.BudgetService.Authorization.PolicyProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

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
            // 409 with the selectable roles when a request fails specifically on acting-role selection
            // (requirement 3, design §4).
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, BudgetAuthorizationResultHandler>();

            return services;
        }
    }
}

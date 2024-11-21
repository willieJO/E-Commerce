using Microsoft.AspNetCore.Authentication;

namespace e_commerce.Infra
{
    public static class DependencyInjectioncs
    {
        public static IServiceCollection AddDi(this IServiceCollection services)
        {
            services.AddScoped<AuthenticationService>();       
            
            services.AddHttpContextAccessor();
            return services;
        }
    }
}

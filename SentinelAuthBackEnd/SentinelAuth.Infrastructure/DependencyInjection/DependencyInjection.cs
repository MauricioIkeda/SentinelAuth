using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Infrastructure.Data;
using SentinelAuth.Infrastructure.Repositories;
using SentinelAuth.Infrastructure.Security;

namespace SentinelAuth.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection("Jwt"))
                .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "Jwt:SecretKey é obrigatório.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer é obrigatório.")
                .ValidateOnStart();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IApplicationClientRepository, ApplicationClientRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPasswordHasher, MicrosoftPasswordHasher>();
            services.AddScoped<IUserApplicationRoleRepository, UserApplicationRoleRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            services.AddScoped<IAdminReadRepository, AdminReadRepository>();

            return services;
        }
    }
}

using Scalar.AspNetCore;
using SentinelAuth.Application.DependencyInjection;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Domain.ValueObjects;
using SentinelAuth.Infrastructure.Data;
using SentinelAuth.Infrastructure.DependencyInjection;
using SentinelAuth.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SentinelAuth.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("SentinelAuthFrontend", policy =>
                {
                    var allowedOrigins = builder.Configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>()
                        ?? new[]
                        {
                            "http://localhost:5173",
                            "http://127.0.0.1:5173"
                        };

                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtSection = builder.Configuration.GetSection("Jwt");
                    var secretKey = jwtSection["SecretKey"] ?? string.Empty;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        RoleClaimType = ClaimTypes.Role
                    };
                });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ApplicationRoleAssignment", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_use", "client_credentials");
                    policy.RequireAssertion(context =>
                    {
                        var scopes = context.User.FindFirst("scope")?.Value
                            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            ?? Array.Empty<string>();

                        return scopes.Contains("roles:assign", StringComparer.OrdinalIgnoreCase);
                    });
                });

                options.AddPolicy("SentinelAdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        if (context.User.IsInRole("SentinelAdmin") ||
                            context.User.IsInRole("SentinelSuperAdmin"))
                        {
                            return true;
                        }

                        var scopes = context.User.FindFirst("scope")?.Value
                            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            ?? Array.Empty<string>();

                        return scopes.Contains("sentinel:admin", StringComparer.OrdinalIgnoreCase);
                    });
                });
            });

            var app = builder.Build();

            await SeedIngressinhosClientAsync(app);
            await SeedSentinelAdminAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors("SentinelAuthFrontend");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task SeedIngressinhosClientAsync(WebApplication app)
        {
            if (!app.Configuration.GetValue<bool>("Seed:Ingressinhos:Enabled"))
            {
                return;
            }

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            const string clientId = "ingressinhos-api";
            var applicationClient = await dbContext.ApplicationClients
                .FirstOrDefaultAsync(client => client.ClientId == clientId);

            if (applicationClient is null)
            {
                var clientSecret = app.Configuration["Seed:Ingressinhos:ClientSecret"];
                var clientSecretHash = string.IsNullOrWhiteSpace(clientSecret)
                    ? null
                    : HashClientSecret(clientSecret);

                var createdClient = ApplicationClient.Create(
                    "Ingressinhos API",
                    clientId,
                    "Ingressinhos.API",
                    clientSecretHash,
                    allowRoleAssignment: !string.IsNullOrWhiteSpace(clientSecretHash)
                );

                if (createdClient.IsFailure)
                {
                    throw new InvalidOperationException(createdClient.Error.Message);
                }

                applicationClient = createdClient.Value;
                await dbContext.ApplicationClients.AddAsync(applicationClient);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                var clientSecret = app.Configuration["Seed:Ingressinhos:ClientSecret"];
                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    applicationClient.ConfigureClientSecret(HashClientSecret(clientSecret));
                    applicationClient.SetRoleAssignmentPermission(true);
                    await dbContext.SaveChangesAsync();
                }
            }

            foreach (var roleName in new[] { "Admin", "Seller", "Client" })
            {
                var normalizedRoleName = roleName.ToUpperInvariant();
                var roleExists = await dbContext.Roles.AnyAsync(role =>
                    role.ApplicationClientId == applicationClient.Id
                    && role.NormalizedName == normalizedRoleName
                );

                if (roleExists)
                {
                    continue;
                }

                var role = Role.Create(applicationClient.Id, roleName);
                if (role.IsFailure)
                {
                    throw new InvalidOperationException(role.Error.Message);
                }

                await dbContext.Roles.AddAsync(role.Value);
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedSentinelAdminAsync(WebApplication app)
        {
            if (!app.Configuration.GetValue<bool>("Bootstrap:SuperAdmin:Enabled"))
            {
                return;
            }

            var emailValue = app.Configuration["Bootstrap:SuperAdmin:Email"];
            var password = app.Configuration["Bootstrap:SuperAdmin:Password"];
            var name = app.Configuration["Bootstrap:SuperAdmin:Name"] ?? "Sentinel SuperAdmin";

            if (string.IsNullOrWhiteSpace(emailValue) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            const string clientId = "sentinel-auth-admin";
            var applicationClient = await dbContext.ApplicationClients
                .FirstOrDefaultAsync(client => client.ClientId == clientId);

            if (applicationClient is null)
            {
                var createdClient = ApplicationClient.Create(
                    "SentinelAuth Admin",
                    clientId,
                    "SentinelAuth.API"
                );

                if (createdClient.IsFailure)
                {
                    throw new InvalidOperationException(createdClient.Error.Message);
                }

                applicationClient = createdClient.Value;
                await dbContext.ApplicationClients.AddAsync(applicationClient);
                await dbContext.SaveChangesAsync();
            }

            var roleIds = new List<long>();
            foreach (var roleName in new[] { "SentinelSuperAdmin", "SentinelAdmin" })
            {
                var normalizedRoleName = roleName.ToUpperInvariant();
                var role = await dbContext.Roles.FirstOrDefaultAsync(item =>
                    item.ApplicationClientId == applicationClient.Id &&
                    item.NormalizedName == normalizedRoleName
                );

                if (role is null)
                {
                    var createdRole = Role.Create(applicationClient.Id, roleName);
                    if (createdRole.IsFailure)
                    {
                        throw new InvalidOperationException(createdRole.Error.Message);
                    }

                    role = createdRole.Value;
                    await dbContext.Roles.AddAsync(role);
                    await dbContext.SaveChangesAsync();
                }

                roleIds.Add(role.Id);
            }

            var email = Email.Create(emailValue);
            if (email.IsFailure)
            {
                throw new InvalidOperationException(email.Error.Message);
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(item => item.Email.Value == email.Value.Value);
            if (user is null)
            {
                var passwordHasher = new MicrosoftPasswordHasher();
                var createdUser = User.Create(name, email.Value, passwordHasher.HashPassword(password));
                if (createdUser.IsFailure)
                {
                    throw new InvalidOperationException(createdUser.Error.Message);
                }

                user = createdUser.Value;
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }
            else if (!user.IsActive)
            {
                user.Activate();
                await dbContext.SaveChangesAsync();
            }

            foreach (var roleId in roleIds)
            {
                var assignmentExists = await dbContext.UserApplicationRoles.AnyAsync(assignment =>
                    assignment.UserId == user.Id &&
                    assignment.ApplicationClientId == applicationClient.Id &&
                    assignment.RoleId == roleId
                );

                if (assignmentExists)
                {
                    continue;
                }

                var assignment = UserApplicationRole.Create(user.Id, applicationClient.Id, roleId);
                if (assignment.IsFailure)
                {
                    throw new InvalidOperationException(assignment.Error.Message);
                }

                await dbContext.UserApplicationRoles.AddAsync(assignment.Value);
            }

            await dbContext.SaveChangesAsync();
        }

        private static string HashClientSecret(string clientSecret)
        {
            var bytes = Encoding.UTF8.GetBytes(clientSecret);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}

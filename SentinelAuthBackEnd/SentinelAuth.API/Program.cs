using Scalar.AspNetCore;
using SentinelAuth.Application.DependencyInjection;
using SentinelAuth.Domain.Entities;
using SentinelAuth.Infrastructure.Data;
using SentinelAuth.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

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

            var app = builder.Build();

            await SeedIngressinhosClientAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors("SentinelAuthFrontend");

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
                var createdClient = ApplicationClient.Create(
                    "Ingressinhos API",
                    clientId,
                    "Ingressinhos.API"
                );

                if (createdClient.IsFailure)
                {
                    throw new InvalidOperationException(createdClient.Error.Message);
                }

                applicationClient = createdClient.Value;
                await dbContext.ApplicationClients.AddAsync(applicationClient);
                await dbContext.SaveChangesAsync();
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
    }
}

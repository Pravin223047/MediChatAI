using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediChatAI_GraphQl.Shared.Services;

/// <summary>
/// Background service that runs database seeding asynchronously without blocking application startup
/// </summary>
public class SeedDataHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedDataHostedService> _logger;

    public SeedDataHostedService(IServiceProvider serviceProvider, ILogger<SeedDataHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database seeding in background...");

        // Run seeding in background without blocking startup
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Check if seeding is needed (optimization)
                var rolesExist = await roleManager.Roles.AnyAsync(cancellationToken);
                if (!rolesExist)
                {
                    await SeedData.Initialize(context, userManager, roleManager);
                }
                
                // Always initialize conversations for existing appointments
                var conversationService = scope.ServiceProvider.GetRequiredService<ConversationInitializationService>();
                var conversationsCreated = await conversationService.InitializeConversationsForExistingAppointmentsAsync();
                if (conversationsCreated > 0)
                {
                    _logger.LogInformation("Created {Count} conversations for existing appointments", conversationsCreated);
                }
                else
                {
                    _logger.LogInformation("No new conversations needed - all appointments already have messages");
                }
                
                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
            }
        }, cancellationToken);

        // Return immediately without blocking
        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seed Data Hosted Service is stopping");
        return Task.CompletedTask;
    }
}

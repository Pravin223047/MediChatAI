using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Infrastructure.Data;

public static class SeedData
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        try
        {
            // Ensure database exists and is migrated
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return; // Skip seeding if database is not available
            }

            // Create roles
            string[] roles = { "Admin", "Doctor", "Patient" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default admin user
            var adminEmail = "admin@medichat.ai";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed default system settings if not exists
            if (!await context.SystemSettings.AnyAsync())
            {
                context.SystemSettings.Add(new SystemSettings());
                await context.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            // Silently fail during startup if database is not ready
            // This prevents application crash during first-time setup
        }
    }
}
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediChatAI_GraphQl.Shared.Services;

public class DynamicIdentityOptionsConfigurator : IConfigureOptions<IdentityOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public DynamicIdentityOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Configure(IdentityOptions options)
    {
        // Always use default options during configuration
        // Settings will be loaded dynamically at runtime when needed
        ApplyDefaultOptions(options);
    }

    private void ApplyDefaultOptions(IdentityOptions options)
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    }

    private static bool IsDesignTime()
    {
        var args = Environment.GetCommandLineArgs();
        return args.Any(arg => arg.Contains("--design-time") || arg.Contains("migrations") || arg.Contains("ef"));
    }
}

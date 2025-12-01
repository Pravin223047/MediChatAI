using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MediChatAI_BlazorWebAssembly.Core.Components.Guards.Base;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Core.Services.State;
using MediChatAI_BlazorWebAssembly.Core.Services.Theme;
using MediChatAI_BlazorWebAssembly.Core.Services.Session;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Services;
using MediChatAI_BlazorWebAssembly.Features.Profile.Services;
using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace MediChatAI_BlazorWebAssembly.Shared.Layout;

/// <summary>
/// Base class for navbar component that provides real-time authentication state updates
/// </summary>
public class NavbarBase : AuthStateAwareComponent
{
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    [Inject] protected IUserProfileService UserProfileService { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected AppSettingsState AppSettings { get; set; } = default!;
    [Inject] protected ThemeState ThemeState { get; set; } = default!;
    [Inject] protected ISessionTimerService SessionTimer { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    protected bool isMobileMenuOpen = false;
    protected bool isUserMenuOpen = false;
    protected string userEmail = "";
    protected string userName = "";
    protected string userRole = "";
    protected string userProfileImage = "";
    protected bool isLoadingUserData = false;
    protected TimeSpan? sessionTimeRemaining = null;
    protected string sessionTimeDisplay = "";
    private DotNetObjectReference<NavbarBase>? _dotNetRef;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        AppSettings.OnChange += OnSettingsChanged;
        ThemeState.OnChange += OnThemeChanged;
        SessionTimer.OnTimerTick += OnSessionTimerTick;
    }

    private void OnSettingsChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnThemeChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnSessionTimerTick(object? sender, TimeSpan timeRemaining)
    {
        sessionTimeRemaining = timeRemaining;
        UpdateSessionTimeDisplay();
        InvokeAsync(StateHasChanged);
    }

    private void UpdateSessionTimeDisplay()
    {
        if (sessionTimeRemaining.HasValue && sessionTimeRemaining.Value.TotalSeconds > 0)
        {
            var minutes = (int)sessionTimeRemaining.Value.TotalMinutes;
            var seconds = sessionTimeRemaining.Value.Seconds;
            sessionTimeDisplay = $"{minutes:D2}:{seconds:D2}";
        }
        else
        {
            sessionTimeDisplay = "";
        }
    }

    public override void Dispose()
    {
        AppSettings.OnChange -= OnSettingsChanged;
        ThemeState.OnChange -= OnThemeChanged;
        SessionTimer.OnTimerTick -= OnSessionTimerTick;

        // Cleanup JavaScript interop
        _dotNetRef?.Dispose();

        base.Dispose();
    }

    protected override async Task OnAuthenticationStateUpdatedAsync()
    {
        await LoadUserDataAsync();
    }

    protected async Task LoadUserDataAsync()
    {
        if (isLoadingUserData) return; // Prevent concurrent loads

        try
        {
            isLoadingUserData = true;
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            if (authState.User.Identity?.IsAuthenticated == true)
            {
                userEmail = authState.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var firstName = authState.User.FindFirst("firstName")?.Value ?? "";
                var lastName = authState.User.FindFirst("lastName")?.Value ?? "";
                userName = $"{firstName} {lastName}".Trim();
                userRole = authState.User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                // Load user profile image asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var profile = await UserProfileService.GetUserProfileAsync();
                        userProfileImage = profile?.ProfileImage ?? "";
                        await InvokeAsync(StateHasChanged);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading profile image: {ex.Message}");
                        userProfileImage = "";
                    }
                });
            }
            else
            {
                // Clear user data when not authenticated
                ClearUserData();
            }

            // Close any open menus when authentication state changes
            CloseAllMenus();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user data: {ex.Message}");
            ClearUserData();
        }
        finally
        {
            isLoadingUserData = false;
        }
    }

    protected void ClearUserData()
    {
        userEmail = "";
        userName = "";
        userRole = "";
        userProfileImage = "";
    }

    protected void CloseAllMenus()
    {
        isUserMenuOpen = false;
        isMobileMenuOpen = false;
    }

    protected void ToggleMobileMenu()
    {
        isMobileMenuOpen = !isMobileMenuOpen;
        isUserMenuOpen = false;
    }

    protected async Task ToggleUserMenu()
    {
        isUserMenuOpen = !isUserMenuOpen;
        isMobileMenuOpen = false;

        if (isUserMenuOpen)
        {
            await RegisterUserMenuOutsideClickAsync();
        }
        else
        {
            await UnregisterUserMenuOutsideClickAsync();
        }
    }

    [JSInvokable("CloseModal")]
    public async Task CloseUserMenuFromJS()
    {
        if (isUserMenuOpen)
        {
            isUserMenuOpen = false;
            await UnregisterUserMenuOutsideClickAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task RegisterUserMenuOutsideClickAsync()
    {
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("registerOutsideClickHandler", "user-menu-button", "user-menu-dropdown", _dotNetRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering outside click handler: {ex.Message}");
        }
    }

    private async Task UnregisterUserMenuOutsideClickAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("unregisterOutsideClickHandler", "user-menu-dropdown");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unregistering outside click handler: {ex.Message}");
        }
    }

    protected async Task HandleLogout()
    {
        try
        {
            // Unregister outside click handler before logout
            await UnregisterUserMenuOutsideClickAsync();

            await AuthService.LogoutAsync();
            Navigation.NavigateTo("/");
            CloseAllMenus();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during logout: {ex.Message}");
        }
    }

    protected string GetDashboardUrl()
    {
        return userRole switch
        {
            "Patient" => "/patient/dashboard",
            "Doctor" => "/doctor/dashboard",
            "Admin" => "/admin/dashboard",
            _ => "/dashboard"
        };
    }

    protected string GetProfileUrl()
    {
        return userRole switch
        {
            "Patient" => "/patient/profile",
            "Doctor" => "/doctor/profile",
            "Admin" => "/admin/profile",
            _ => "/profile"
        };
    }

    protected string GetUserInitials()
    {
        var names = userName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (names.Length >= 2)
        {
            return $"{names[0][0]}{names[1][0]}".ToUpper();
        }
        else if (names.Length == 1 && names[0].Length > 0)
        {
            return names[0][0].ToString().ToUpper();
        }
        return "U";
    }

    protected string GetRoleGradient()
    {
        return userRole switch
        {
            "Patient" => "from-primary to-accent",
            "Doctor" => "from-primary to-accent",
            "Admin" => "from-primary to-accent",
            _ => "from-primary to-accent"
        };
    }

    protected string GetRoleBadgeColor()
    {
        return userRole switch
        {
            "Patient" => "bg-blue-100 text-blue-800",
            "Doctor" => "bg-emerald-100 text-emerald-800",
            "Admin" => "bg-purple-100 text-purple-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    protected string GetRoleDisplayName()
    {
        return userRole switch
        {
            "Patient" => "Patient",
            "Doctor" => "Doctor",
            "Admin" => "Administrator",
            _ => "User"
        };
    }

    protected string GetImageUrl(string? profileImage)
    {
        if (string.IsNullOrEmpty(profileImage))
            return "";

        // If it's already a full URL, return as is
        if (profileImage.StartsWith("http://") || profileImage.StartsWith("https://"))
            return profileImage;

        // If it's a relative path, prepend the base URL
        return $"http://localhost:5095{profileImage}";
    }
}
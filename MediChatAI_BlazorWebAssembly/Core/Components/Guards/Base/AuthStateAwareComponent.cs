using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MediChatAI_BlazorWebAssembly.Core.Components.Guards.Base;

/// <summary>
/// Base component that automatically updates when authentication state changes.
/// Inherit from this component to get real-time authentication state updates.
/// </summary>
public abstract class AuthStateAwareComponent : ComponentBase, IDisposable
{
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to authentication state changes
        AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        await OnAuthStateInitializedAsync();
    }

    /// <summary>
    /// Called when the component is initialized and when authentication state changes.
    /// Override this method to handle authentication state updates.
    /// </summary>
    protected virtual async Task OnAuthStateInitializedAsync()
    {
        await OnAuthenticationStateUpdatedAsync();
    }

    /// <summary>
    /// Called whenever the authentication state changes.
    /// Override this method to handle authentication state updates.
    /// </summary>
    protected virtual async Task OnAuthenticationStateUpdatedAsync()
    {
        // Default implementation - override in derived classes
        await Task.CompletedTask;
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        try
        {
            await OnAuthenticationStateUpdatedAsync();
            // Force re-render to update the UI
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling authentication state change: {ex.Message}");
        }
    }

    public virtual void Dispose()
    {
        // Unsubscribe from authentication state changes
        AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
using Blazored.LocalStorage;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Services;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Session;

public class SessionTimerService : ISessionTimerService, IDisposable
{
    private readonly ITokenValidationService _tokenValidationService;
    private readonly ILocalStorageService _localStorage;
    private Timer? _timer;
    private bool _isRunning = false;

    public event EventHandler<TimeSpan>? OnTimerTick;
    public event EventHandler? OnTimerStarted;
    public event EventHandler? OnTimerStopped;

    public bool IsRunning => _isRunning;

    public SessionTimerService(
        ITokenValidationService tokenValidationService,
        ILocalStorageService localStorage)
    {
        _tokenValidationService = tokenValidationService;
        _localStorage = localStorage;
    }

    public async Task StartTimerAsync()
    {
        if (_isRunning)
            return;

        var timeRemaining = await GetTimeRemainingAsync();
        if (!timeRemaining.HasValue || timeRemaining.Value.TotalSeconds <= 0)
            return;

        _isRunning = true;
        OnTimerStarted?.Invoke(this, EventArgs.Empty);

        // Emit initial tick immediately
        OnTimerTick?.Invoke(this, timeRemaining.Value);

        // Start timer that ticks every second
        _timer = new Timer(async _ => await TickAsync(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        Console.WriteLine("Session timer started");
    }

    public void StopTimer()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _timer?.Dispose();
        _timer = null;

        OnTimerStopped?.Invoke(this, EventArgs.Empty);
        Console.WriteLine("Session timer stopped");
    }

    public async Task<TimeSpan?> GetTimeRemainingAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            return _tokenValidationService.GetTimeUntilExpiration(token);
        }
        catch
        {
            return null;
        }
    }

    private async Task TickAsync()
    {
        if (!_isRunning)
            return;

        var timeRemaining = await GetTimeRemainingAsync();
        if (!timeRemaining.HasValue || timeRemaining.Value.TotalSeconds <= 0)
        {
            StopTimer();
            return;
        }

        OnTimerTick?.Invoke(this, timeRemaining.Value);
    }

    public void Dispose()
    {
        StopTimer();
    }
}

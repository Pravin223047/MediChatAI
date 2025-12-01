using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Core.Services.UI;

public interface IToastService
{
    event Action<ToastMessage>? OnShow;
    event Action<Guid>? OnHide;

    void ShowSuccess(string message, int duration = 3000);
    void ShowError(string message, int duration = 5000);
    void ShowWarning(string message, int duration = 4000);
    void ShowInfo(string message, int duration = 3000);
    void Hide(Guid toastId);
}

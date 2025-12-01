using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Core.Services.UI
{
    public class ToastService : IToastService
    {
        public event Action<ToastMessage>? OnShow;
        public event Action<Guid>? OnHide;

        public void ShowSuccess(string message, int duration = 3000)
        {
            var toast = new ToastMessage
            {
                Id = Guid.NewGuid(),
                Message = message,
                Type = ToastType.Success,
                Duration = duration
            };
            OnShow?.Invoke(toast);
        }

        public void ShowError(string message, int duration = 5000)
        {
            var toast = new ToastMessage
            {
                Id = Guid.NewGuid(),
                Message = message,
                Type = ToastType.Error,
                Duration = duration
            };
            OnShow?.Invoke(toast);
        }

        public void ShowWarning(string message, int duration = 4000)
        {
            var toast = new ToastMessage
            {
                Id = Guid.NewGuid(),
                Message = message,
                Type = ToastType.Warning,
                Duration = duration
            };
            OnShow?.Invoke(toast);
        }

        public void ShowInfo(string message, int duration = 3000)
        {
            var toast = new ToastMessage
            {
                Id = Guid.NewGuid(),
                Message = message,
                Type = ToastType.Info,
                Duration = duration
            };
            OnShow?.Invoke(toast);
        }

        public void Hide(Guid toastId)
        {
            OnHide?.Invoke(toastId);
        }
    }
}

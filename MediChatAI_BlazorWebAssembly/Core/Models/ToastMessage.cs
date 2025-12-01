namespace MediChatAI_BlazorWebAssembly.Core.Models;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; }
    public int Duration { get; set; } = 3000;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

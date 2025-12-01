namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface ISmartReplyService
{
    List<string> GenerateSuggestions(string messageContent, bool isFromDoctor);
}

public class SmartReplyService : ISmartReplyService
{
    public List<string> GenerateSuggestions(string messageContent, bool isFromDoctor)
    {
        var suggestions = new List<string>();
        var lowerContent = messageContent.ToLower();

        if (isFromDoctor)
        {
            // Responses to doctor messages
            if (lowerContent.Contains("appointment") || lowerContent.Contains("schedule"))
            {
                suggestions.AddRange(new[]
                {
                    "Yes, that works for me",
                    "Can we reschedule?",
                    "What times are available?",
                    "I'll confirm shortly"
                });
            }
            else if (lowerContent.Contains("medication") || lowerContent.Contains("prescription"))
            {
                suggestions.AddRange(new[]
                {
                    "Thank you, I understand",
                    "Any side effects I should watch for?",
                    "How long should I take this?",
                    "Can I take this with food?"
                });
            }
            else if (lowerContent.Contains("test") || lowerContent.Contains("lab") || lowerContent.Contains("result"))
            {
                suggestions.AddRange(new[]
                {
                    "When will results be ready?",
                    "Do I need to fast?",
                    "Where should I go for the test?",
                    "Thank you for letting me know"
                });
            }
            else if (lowerContent.Contains("feel") || lowerContent.Contains("symptom"))
            {
                suggestions.AddRange(new[]
                {
                    "I'm feeling better, thank you",
                    "Still experiencing some symptoms",
                    "Should I be concerned?",
                    "When should I follow up?"
                });
            }
            else if (lowerContent.Contains("?"))
            {
                suggestions.AddRange(new[]
                {
                    "Yes, that's correct",
                    "No, not exactly",
                    "Let me check and get back to you",
                    "I'm not sure"
                });
            }
            else
            {
                suggestions.AddRange(new[]
                {
                    "Thank you, Doctor",
                    "I understand",
                    "Got it, thanks!",
                    "I have a follow-up question"
                });
            }
        }
        else
        {
            // Responses to patient messages (if implemented for doctor role)
            if (lowerContent.Contains("pain") || lowerContent.Contains("hurt"))
            {
                suggestions.AddRange(new[]
                {
                    "On a scale of 1-10, how severe?",
                    "When did the pain start?",
                    "Is it constant or intermittent?",
                    "Any other symptoms?"
                });
            }
            else if (lowerContent.Contains("thanks") || lowerContent.Contains("thank you"))
            {
                suggestions.AddRange(new[]
                {
                    "You're welcome!",
                    "Happy to help",
                    "Take care",
                    "Feel better soon"
                });
            }
            else
            {
                suggestions.AddRange(new[]
                {
                    "I understand",
                    "Let me know if you need anything",
                    "Keep me updated",
                    "Take care"
                });
            }
        }

        return suggestions.Take(4).ToList();
    }
}

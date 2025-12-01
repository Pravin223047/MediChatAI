using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediChatAI_GraphQl.Shared.Services;

namespace MediChatAI_GraphQl.Shared.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationController : ControllerBase
{
    private readonly ConversationInitializationService _conversationService;
    private readonly ILogger<ConversationController> _logger;

    public ConversationController(ConversationInitializationService conversationService, ILogger<ConversationController> logger)
    {
        _conversationService = conversationService;
        _logger = logger;
    }

    /// <summary>
    /// Initialize conversations for existing appointments - temporary endpoint for fixing the issue
    /// </summary>
    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeConversations()
    {
        try
        {
            var count = await _conversationService.InitializeConversationsForExistingAppointmentsAsync();
            return Ok(new { success = true, conversationsCreated = count, message = $"Created {count} conversations for existing appointments" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing conversations");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}
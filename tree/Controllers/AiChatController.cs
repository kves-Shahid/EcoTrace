using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoTraceApp.Services;
using EcoTraceApp.Data;
using EcoTraceApp.Models;

namespace EcoTraceApp.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        private readonly EcoTraceAiService _aiService;
        private readonly UserManager<IdentityUser> _userManager;

      
        private readonly AppDbContext _context;

       
        public AiChatController(EcoTraceAiService aiService, UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _aiService = aiService;
            _userManager = userManager;
            _context = context; 
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var history = await _context.AiChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new { isAi = m.IsFromAi, text = m.MessageText })
                .ToListAsync();

            return Ok(history);
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskAi([FromBody] ChatRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            
            var history = await _context.AiChatMessages
                .Where(m => m.UserId == user.Id)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            
            var userMsg = new AiChatMessage { UserId = user.Id, IsFromAi = false, MessageText = request.Message };
            _context.AiChatMessages.Add(userMsg);
            await _context.SaveChangesAsync();

      
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var responseText = await _aiService.GetChatResponseAsync(request.Message, history, isAdmin, user.Id);

            var aiMsg = new AiChatMessage { UserId = user.Id, IsFromAi = true, MessageText = responseText };
            _context.AiChatMessages.Add(aiMsg);
            await _context.SaveChangesAsync();

            return Ok(new { reply = responseText });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
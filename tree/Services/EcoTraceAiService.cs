using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EcoTraceApp.Data;
using EcoTraceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoTraceApp.Services
{
    public class EcoTraceAiService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public EcoTraceAiService(AppDbContext context, IConfiguration config, HttpClient httpClient)
        {
            _context = context;
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<string> GetChatResponseAsync(string userMessage, List<AiChatMessage> history, bool isAdmin, string userId)
        {
            var activeEvents = await _context.Events
                .Include(e => e.Registrations)
                .Where(e => !e.IsCompleted)
                .Select(e => $"Title: {e.Title}, Date: {e.EventDate:yyyy-MM-dd}, Location: {e.LocationName}, Joined: {e.Registrations.Count}/{e.MaxVolunteers}")
                .ToListAsync();

            string dbContext = string.Join(" | ", activeEvents);

            
            string systemPrompt = isAdmin
                ? $@"You are the EcoTrace Admin Assistant. You manage social work and tree-planting events.
                  Current Active Events in Database: {dbContext}
                  If the admin asks to CREATE or ORGANIZE an event, output EXACTLY this JSON format inside a special block. Replace the placeholders with REAL data based on the user's prompt. 
                  IMPORTANT: For EventDate, you MUST use a real future date in standard ISO 8601 format (Example: ""2026-03-15T09:00:00"").
                  [CREATE_EVENT]
                  {{ ""Title"": ""..."", ""Description"": ""..."", ""EventDate"": ""2026-03-15T09:00:00"", ""EventType"": ""Tree Planting"", ""LocationName"": ""..."", ""MaxVolunteers"": 50 }}
                  [/CREATE_EVENT]
                  Otherwise, answer normally."
                : $@"You are the EcoTrace Volunteer Assistant. You ONLY discuss trees, social work, and active events.
                  Current Active Events in Database: {dbContext}
                  You DO NOT have permission to create events. If asked to create one, politely decline. Be concise.";

            var chatContents = new List<object>();
            foreach (var msg in history.OrderBy(h => h.Timestamp))
            {
                chatContents.Add(new
                {
                    role = msg.IsFromAi ? "model" : "user",
                    parts = new[] { new { text = msg.MessageText } }
                });
            }

            chatContents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var apiKey = _config["Gemini:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                return "⚠️ Configuration Error: The Gemini API key is missing from appsettings.json.";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = chatContents
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var jsonString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonString);

            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                var errorMessage = errorElement.GetProperty("message").GetString();
                return $"⚠️ **Google API Error:** {errorMessage}";
            }

            var replyText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";

            if (isAdmin)
            {
                var match = Regex.Match(replyText, @"\[CREATE_EVENT\](.*?)\[\/CREATE_EVENT\]", RegexOptions.Singleline);
                if (match.Success)
                {
                    try
                    {
                        var eventJson = match.Groups[1].Value.Trim();

                     
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var newEvent = JsonSerializer.Deserialize<Event>(eventJson, options);

                        if (newEvent != null)
                        {
                            newEvent.CreatorId = userId;
                            newEvent.CreatedAt = DateTime.UtcNow;
                            _context.Events.Add(newEvent);
                            await _context.SaveChangesAsync();
                            return $"✅ **Success!** I have automatically created the event '{newEvent.Title}' for you.";
                        }
                    }
                    catch (Exception ex)
                    {
                        return $"I tried to create the event, but ran into an error: {ex.Message}";
                    }
                }
            }

            return replyText;
        }
    }
}
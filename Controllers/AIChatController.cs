using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LTMS.Controllers
{
    [Authorize]
    public class AIChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AIChatController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("api/aichat/send")]
        public async Task<IActionResult> SendMessage([FromBody] AIChatMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Content))
                return BadRequest(new { error = "Empty message" });

            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return BadRequest(new { error = "Gemini API key missing" });

            var client = _httpClientFactory.CreateClient();

            string preamble = @"
You are an AI assistant for the Leather Trading Management System (LTMS).

System Features:
- Sellers manage leather inventory
- Buyers post demands
- Sellers bid on demands
- Buyers accept/reject bids
- Orders & payments are tracked
- Reviews after order completion

Instructions:
- Give step-by-step guidance
- Be concise and clear
- If unsure, advise contacting support
";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{preamble}\n\nUser: {message.Content}" }
                        }
                    }
                }
            };

            var url =
                $"https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key={apiKey}";

            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(url, content);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { error = raw });

                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                if (!root.TryGetProperty("candidates", out var candidates) ||
                    candidates.GetArrayLength() == 0)
                {
                    return Ok(new { reply = "AI could not generate a response." });
                }

                var reply = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return Ok(new { reply });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class AIChatMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LTMS.Controllers
{
    public class RealtimeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RealtimeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("api/realtime/session")]
        public async Task<IActionResult> GetEphemeralToken()
        {
            var client = _httpClientFactory.CreateClient();
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("OpenAI API key is not configured");
            }

            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestContent = new StringContent(JsonSerializer.Serialize(new
            {
                model = "gpt-4o-realtime-preview-2024-12-17",
                voice = "verse"
            }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/realtime/sessions", requestContent);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, json);
            }

            return Content(json, "application/json");
        }

        [HttpGet("realtime")]
        public IActionResult Index()
        {
            return View();
        }
    }
} 
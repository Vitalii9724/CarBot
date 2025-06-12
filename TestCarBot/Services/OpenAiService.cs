using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TestCarBot.Services
{
    public class OpenAiService
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private readonly string _systemPrompt;
        private readonly string _botSystemPrompt;

        public OpenAiService(IConfiguration config)
        {
            _apiKey = config["OpenAI:ApiKey"]!;
            _model = config["OpenAI:Model"]!;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://openrouter.ai/") };

            _systemPrompt = """
            Ти працюєш як Telegram-бот для автострахування. Відповідай стисло та по суті, не виходь за межі теми автострахування та документів. Якщо питання не стосується страхування, скажи "Я можу допомогти лише зі страхуванням авто 🚗". Уникай довгих пояснень. Відповідай українською.
            """;

            _botSystemPrompt = """
            Ти — Telegram-бот для автострахування. Твоє завдання — генерувати короткі, дружні та зрозумілі повідомлення для користувача українською мовою на основі наданого сценарію. Не додавай нічого зайвого, лише текст повідомлення.
            """;
        }

        public async Task<string> GenerateUserReplyAsync(string userMessage)
        {
            return await SendRequestAsync(_systemPrompt, userMessage);
        }

        public async Task<string> GenerateBotResponseAsync(string scenario, Dictionary<string, string>? data = null)
        {
            if (data != null)
            {
                foreach (var entry in data)
                {
                    scenario = scenario.Replace($"{{{entry.Key}}}", entry.Value);
                }
            }
            return await SendRequestAsync(_botSystemPrompt, scenario);
        }

        private async Task<string> SendRequestAsync(string systemContent, string userContent)
        {
            try
            {
                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = systemContent },
                        new { role = "user", content = userContent }
                    }
                };

                var requestJson = JsonSerializer.Serialize(requestBody);
                var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/chat/completions")
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                request.Headers.Add("HTTP-Referer", "https://t.me/Car_Insurance_Sales_2131_Bot");
                request.Headers.Add("X-Title", "InsuranceBot");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ OpenRouter помилка {response.StatusCode}: {responseContent}");
                    return "Вибачте, сталася технічна помилка. Спробуйте пізніше.";
                }

                var json = JsonDocument.Parse(responseContent);
                var reply = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                return reply ?? "Не вдалося згенерувати відповідь.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Виняток OpenRouter: {ex.Message}");
                return "Помилка при з'єднанні з сервісом. Будь ласка, спробуйте знову за хвилину.";
            }
        }
    }
}
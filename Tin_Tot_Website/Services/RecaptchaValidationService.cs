using System.Text.Json;

namespace Tin_Tot_Website.Services
{
    public class RecaptchaValidationService : IRecaptchaValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RecaptchaValidationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public bool IsEnabled => bool.TryParse(_configuration["Recaptcha:Enabled"], out var enabled) && enabled;

        public async Task<bool> ValidateAsync(string? token, string? remoteIp)
        {
            if (!IsEnabled)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var secret = _configuration["Recaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                return false;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.google.com/recaptcha/api/siteverify")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secret,
                    ["response"] = token,
                    ["remoteip"] = remoteIp ?? string.Empty
                })
            };

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<RecaptchaVerifyResponse>(stream);
            if (payload is null || !payload.Success)
            {
                return false;
            }

            var minScore = double.TryParse(_configuration["Recaptcha:MinScore"], out var score) ? score : 0.5;
            if (payload.Score.HasValue && payload.Score.Value < minScore)
            {
                return false;
            }

            return true;
        }

        private sealed class RecaptchaVerifyResponse
        {
            public bool Success { get; set; }
            public double? Score { get; set; }
        }
    }
}

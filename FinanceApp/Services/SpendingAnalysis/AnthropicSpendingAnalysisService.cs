using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Services.SpendingAnalysis
{
    /// <summary>
    /// Spending analysis backed by the Anthropic Messages API
    /// (POST /v1/messages). Registered as a typed HttpClient; base address,
    /// auth headers and retry/backoff live in the HttpClient pipeline
    /// configured in Program.cs.
    /// </summary>
    public class AnthropicSpendingAnalysisService : ISpendingAnalysisService
    {
        public const string DefaultModel = "claude-haiku-4-5-20251001";

        // Insights/recommendations are short prose; cap output deliberately
        // to bound per-request cost on the portfolio demo.
        private const int MaxTokens = 1024;

        private const string AnalysisSystemPrompt =
            "You are a personal finance analyst. You are given a list of a user's " +
            "transactions. Identify concrete spending patterns and anomalies, and " +
            "summarize them in short, plain-language paragraphs for the user. " +
            "All amounts are in the currency shown with each transaction.";

        private const string RecommendationsSystemPrompt =
            "You are a personal finance coach. Provide practical, prioritized " +
            "recommendations for better personal financial management in short, " +
            "plain-language paragraphs or bullet points.";

        private readonly HttpClient _httpClient;
        private readonly ILogger<AnthropicSpendingAnalysisService> _logger;
        private readonly string _model;

        public AnthropicSpendingAnalysisService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AnthropicSpendingAnalysisService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _model = configuration["Anthropic:Model"] ?? DefaultModel;
        }

        public Task<string> AnalyzeSpendingHabitsAsync(string prompt, CancellationToken cancellationToken = default)
            => CreateMessageAsync(AnalysisSystemPrompt, prompt, cancellationToken);

        public Task<string> GenerateRecommendationsAsync(string prompt, CancellationToken cancellationToken = default)
            => CreateMessageAsync(RecommendationsSystemPrompt, prompt, cancellationToken);

        private async Task<string> CreateMessageAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            {
                throw new InvalidOperationException(
                    "Anthropic:ApiKey is not configured. Set it in appsettings.Development.json " +
                    "(gitignored) for local runs or via the Anthropic__ApiKey environment variable.");
            }

            var payload = new
            {
                model = _model,
                max_tokens = MaxTokens,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            _logger.LogInformation("[AnthropicSpendingAnalysisService] Requesting completion from model {Model}", _model);

            // Transient failures (429/5xx) are retried by the standard
            // resilience handler on this client; a failure here is final.
            using HttpResponseMessage response = await _httpClient.PostAsync("/v1/messages", content, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "[AnthropicSpendingAnalysisService] Anthropic API returned {StatusCode}. Body: {Body}",
                    (int)response.StatusCode, body);
                throw new HttpRequestException($"Anthropic API returned {(int)response.StatusCode} ({response.StatusCode}).");
            }

            using JsonDocument document = JsonDocument.Parse(body);
            var result = new StringBuilder();
            foreach (JsonElement block in document.RootElement.GetProperty("content").EnumerateArray())
            {
                if (block.GetProperty("type").GetString() == "text")
                {
                    result.Append(block.GetProperty("text").GetString());
                }
            }

            JsonElement usage = document.RootElement.GetProperty("usage");
            _logger.LogInformation(
                "[AnthropicSpendingAnalysisService] Completion received: {InputTokens} input / {OutputTokens} output tokens",
                usage.GetProperty("input_tokens").GetInt64(), usage.GetProperty("output_tokens").GetInt64());

            return result.ToString().Trim();
        }
    }
}

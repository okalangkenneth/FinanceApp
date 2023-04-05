using FinanceApp.Exceptions;
using FinanceApp.Services.HttpRequestMessageExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;


        public OpenAIService(HttpClient httpClient, string apiKey, ILogger<OpenAIService> logger)
        {
            httpClient.BaseAddress = new Uri("https://api.openai.com");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<string> AnalyzeSpendingHabitsAsync(string prompt, string apiKey, string endpoint)
        {
            string response = await SendRequestWithExponentialBackoffAsync(_httpClient, apiKey, endpoint, prompt);

            JObject jsonResponse = JObject.Parse(response);
            string result = jsonResponse["choices"][0]["text"].ToString().Trim();

            return result;
        }
        private async Task<string> SendRequestWithExponentialBackoffAsync(HttpClient httpClient, string apiKey, string endpoint, string prompt)
        {
            int initialDelay = 1000; // 1 second
            int maxDelay = 60000; // 60 seconds
            int maxRetries = 5;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    request.Content = new StringContent(JsonConvert.SerializeObject(new { prompt = prompt, max_tokens = 150 }), Encoding.UTF8, "application/json");


                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        int delay = (int)(Math.Pow(2, retry) * initialDelay);
                        delay = Math.Min(delay, maxDelay);

                        await Task.Delay(delay);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
                    {
                        throw new CreditExhaustedException($"API returned a {response.StatusCode} status code. Subscription limit reached or payment required.");
                    }
                    else
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"API returned a {response.StatusCode} status code. Response body: {responseBody}");

                        throw new HttpRequestException($"API returned a {response.StatusCode} status code.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (retry == maxRetries - 1)
                    {
                        throw ex;
                    }
                }
            }

            throw new HttpRequestException("Max retries reached.");
        }
        private HttpRequestMessage CloneHttpRequestMessage(HttpRequestMessage requestTemplate)
        {
            var clone = new HttpRequestMessage(requestTemplate.Method, requestTemplate.RequestUri);
            foreach (var header in requestTemplate.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (requestTemplate.Content != null)
            {
                var content = requestTemplate.Content.ReadAsStringAsync().Result;
                clone.Content = new StringContent(content, Encoding.UTF8, requestTemplate.Content.Headers.ContentType.MediaType);
            }

            return clone;


        }
        public async Task<string> GenerateRecommendationsAsync(string prompt, string apiKey, string endpoint)
        {
            string response = await SendRequestWithExponentialBackoffAsync(_httpClient, apiKey, endpoint, prompt);

            JObject jsonResponse = JObject.Parse(response);
            string result = jsonResponse["choices"][0]["text"].ToString().Trim();

            return result;
        }

    }
}
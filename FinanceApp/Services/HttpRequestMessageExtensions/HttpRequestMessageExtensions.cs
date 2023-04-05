using System.Net.Http;

namespace FinanceApp.Services.HttpRequestMessageExtensions
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            HttpRequestMessage clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                clone.Content = new StringContent(request.Content.ReadAsStringAsync().Result);
                if (request.Content.Headers != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        clone.Content.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Version = request.Version;

            return clone;
        }
    }

}

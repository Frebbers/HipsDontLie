using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HipsDontLie.Client.Handlers
{
    public class ApiErrorHandler : DelegatingHandler
    {
        private record ApiMessageBody(string? Message);
        protected override async Task<HttpResponseMessage> SendAsync(
           HttpRequestMessage request,
           CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
                return response;

            string? raw = null;
            ApiMessageBody? parsed = null;

            if (response.Content != null)
            {
                raw = await response.Content.ReadAsStringAsync(cancellationToken);

                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (!string.IsNullOrWhiteSpace(raw) &&
                    contentType != null &&
                    contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        parsed = JsonSerializer.Deserialize<ApiMessageBody>(
                            raw,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                    }
                    catch (JsonException)
                    {
                        // Ignore JSON parsing errors
                    }
                }
            }

            var message = parsed?.Message
                ?? (!string.IsNullOrWhiteSpace(raw) ? raw : response.ReasonPhrase ?? "Unknown error");

            
            Console.WriteLine($"API error {response.StatusCode} for {request.Method} {request.RequestUri}: {message}");

            throw new ApiException(response.StatusCode, message);
        }
    }

    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApiException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}

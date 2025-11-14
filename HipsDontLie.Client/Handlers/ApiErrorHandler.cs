using System.Net;
using System.Net.Http.Json;

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

            var error = await response.Content.ReadFromJsonAsync<ApiMessageBody>(cancellationToken) ?? new ApiMessageBody("Unknown error");

            throw new ApiException(response.StatusCode, error.Message ?? "Unknown error");
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

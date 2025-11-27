using System.Net.Http.Headers;

namespace HipsDontLie.Client.Handlers
{
    public sealed class ApiRequestHandler : DelegatingHandler
    {
        private readonly CustomAuthStateProvider _auth;
        private static readonly string ApiHost = new Uri("https://hipsdontlie.live/").Host;

        public ApiRequestHandler(CustomAuthStateProvider auth) => _auth = auth;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _auth.GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

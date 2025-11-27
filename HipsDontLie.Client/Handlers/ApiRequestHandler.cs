namespace HipsDontLie.Client.Handlers
{
    public sealed class ApiRequestHandler : DelegatingHandler
    {
        private readonly CustomAuthStateProvider _auth;

        public ApiRequestHandler(CustomAuthStateProvider auth) => _auth = auth;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
                var token = await _auth.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.ParseAdd("application/json");
            return await base.SendAsync(request, ct);
        }
    }
}

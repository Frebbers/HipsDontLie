namespace HipsDontLie.Client.Services
{
    public sealed class APIDelegatingHandler : DelegatingHandler
    {
        private readonly CustomAuthStateProvider _auth;
        private static readonly string ApiHost = new Uri("https://localhost:7191/").Host;

        public APIDelegatingHandler(CustomAuthStateProvider auth) => _auth = auth;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            //To avoid we dont attach the token to a malicious url
            if (request.RequestUri?.Host == ApiHost)
            {
                var token = await _auth.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            request.Headers.Accept.ParseAdd("application/json");
            return await base.SendAsync(request, ct);
        }
    }
}

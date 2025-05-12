using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GameTogetherAPI.Test.Old.Factories
{
    public class APIFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //We don't implement anything here because we don't need to configure the host right now
        }
    }
}
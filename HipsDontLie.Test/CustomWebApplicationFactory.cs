using Microsoft.AspNetCore.Mvc.Testing;
using HipsDontLie.Server;

namespace HipsDontLie.Test
{
    public class CustomWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HipsDontLie.Test.Old.Drivers;
using HipsDontLie.Test.Old.Factories;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace GameTogetherAPI.Test.Old.Fixtures
{
    public class APITestContext
    {
        public HttpClient Client { get; set; }
        public APIFactory<Program> Factory { get; set; }

    }
}

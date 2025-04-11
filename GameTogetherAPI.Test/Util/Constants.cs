using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GameTogetherAPI.Test.Util
{
    internal static class APIConstants
    {
        public const string BaseAddress = "http://localhost:5000";
        public const string DockerAddress = "http://localhost:8080";

        public const string TestEmail = "user@example.com";
        public const string TestPassword = "Password123";
        
        public const string TestEmail1 = "user2@example.com";
        public const string TestPassword1 = "Password1234";
    }
}

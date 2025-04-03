using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CucumberExpressions.Ast;

namespace GameTogetherAPI.Test.Models
{
    internal class APIResponse
    {
        public string token { get; set; }
        public string email { get; set; }
    }
}

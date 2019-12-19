using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DevTestMHS.Controllers
{
    public class TestController : ApiController
    {
        [Route("api/test")]
        [HttpGet]
        public string Index()
        {
            return "Hello World";
        }
    }
}

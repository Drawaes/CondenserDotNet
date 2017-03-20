using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolSwitchingTest
{
    [Route("[controller]")]
    public class TestController:Controller
    {
        [HttpGet]
        public IEnumerable<TestResponse> Get()
        {
            for (int i = 0; i < 1000; i++)
            {
                var newResponse = new TestResponse()
                {
                    SomeProperty = i.ToString()
                };
                yield return newResponse;
            }
        }
    }

    public class TestResponse
    {
        public string SomeProperty { get; set; }
    }
        

}

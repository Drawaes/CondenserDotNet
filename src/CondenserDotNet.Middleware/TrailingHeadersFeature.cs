using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CondenserDotNet.Middleware
{
    public class TrailingHeadersFeature : ITrailingHeadersFeature
    {
        private List<Tuple<string, Func<string>>> _headers = new List<Tuple<string, Func<string>>>();
        private HttpContext _context; 

        public TrailingHeadersFeature(HttpContext context)
        {
            _context = context;
        }

        public List<Tuple<string, Func<string>>> Headers => _headers;

        public void RegisterHeader(string name, Func<string> contentCallback)
        {
            for(int i = 0; i < _headers.Count;i++)
            {
                var headerItem = _headers[i];
                if(string.Compare(name,headerItem.Item1, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _headers[i] = Tuple.Create<string,Func<string>>(headerItem.Item1, () => headerItem.Item2() + ", " + contentCallback());
                    return;
                }
            }
            _headers.Add(Tuple.Create(name,contentCallback));
            _context.Response.Headers["Trailer"] = new StringValues(_headers.Select(h => h.Item1).ToArray());
        }
    }
}

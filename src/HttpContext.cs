using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server
{
    public class HttpContext
    {
        public string Action { get; set; }

        public string Path { get; set; }

        public string Body { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public static HttpContext ParseRequest(string request)
        {
            if (string.IsNullOrEmpty(request)) throw new ArgumentNullException(nameof(request));

                var tokens = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                var rtokens = tokens[0].Split(' ');
                var path = rtokens[1];
                var action = rtokens[0];

                var body = tokens[^1].Replace("\0", "");

                var Headers = new Dictionary<string, string>();
                int i = 1;

                while (!string.IsNullOrEmpty(tokens[i]))
                {
                    var line = tokens[i];

                    var kv = line.Split(':');
                    Headers.Add(kv[0].Trim(), kv[1].Trim());
                    i++;
                }

            return new HttpContext { Headers = Headers, Body = body, Path = path, Action = action };
        }
    }
}

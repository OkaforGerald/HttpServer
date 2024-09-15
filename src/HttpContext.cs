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

        //This assumes only one parameter in route, Come back
        public string Parameter { get; set; }

        public string Body { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public static HttpContext ParseRequest(string request)
        {
            if (string.IsNullOrEmpty(request)) throw new ArgumentNullException(nameof(request));

                var tokens = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                var rtokens = tokens[0].Split(' ');
                var path = rtokens[1];
                var pathTokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                string param = "";
                if (pathTokens.Length == 0) {
                    path = "/";
                }
                else
                {
                    path = $"/{pathTokens[0]}";
                    if (pathTokens.Length == 2)
                    {
                        param = pathTokens[^1];
                    }
                }
                var action = rtokens[0];

                var body = tokens[^1].Replace("\0", "");

                var Headers = new Dictionary<string, string>();
                int i = 1;

                while (!string.IsNullOrEmpty(tokens[i]))
                {
                    var line = tokens[i];

                    var kv = line.Split(':');
                    if (kv[0].Trim().Equals("Accept-Encoding") && kv[1].Contains(','))
                    {
                        var gzipExists = kv[1].Split(',').Any(kv => kv.Trim().Equals("gzip"));
                        if(gzipExists)
                        {
                            Headers.Add(kv[0].Trim(), "gzip");
                        }
                    }
                    else if(kv[0].Trim().Equals("Accept-Encoding"))
                    {
                        if (kv[1].Trim().Equals("gzip")) Headers.Add(kv[0].Trim(), "gzip");
                    }
                    else
                    {
                        Headers.Add(kv[0].Trim(), kv[1].Trim());
                    }    
                    i++;
               }

            return new HttpContext { Headers = Headers, Body = body, Path = path, Action = action, Parameter = param };
        }
    }
}

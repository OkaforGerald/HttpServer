using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Server server = new Server();

            server.MapGet("/", body => server.PrepareOkResponse() );
            server.MapGet("/user-agent", ctx => server.PrepareOkResponse(ContentType: "text/plain", Length: ctx.Headers["User-Agent"].Length, Body: ctx.Headers["User-Agent"]));
            //server.MapPost("/echo", )
            server.Run();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
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

            server.MapGet("/user-agent", ctx => server.PrepareOkResponse(ContentType: "text/plain", 
                Length: ctx.Headers["User-Agent"].Length, 
                Body: ctx.Headers["User-Agent"]));

            server.MapGet("/echo", ctx =>
            {
                var compression = ctx.Headers.TryGetValue("Accept-Encoding", out string? encoding);
                return encoding is null || !encoding.Equals("gzip", StringComparison.OrdinalIgnoreCase) ? server.PrepareOkResponse(ContentType: "text/plain",
                    Length: ctx.Parameter.Length,
                    Body: ctx.Parameter) : server.PrepareOkResponse(ContentType: "text/plain",
                    Length: ctx.Parameter.Length,
                    Body: ctx.Parameter, Encoding: encoding);
            });

            server.MapGet("/files", ctx =>
            {
                var arguments = Environment.GetCommandLineArgs();
                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), arguments[2], ctx.Parameter)))
                {
                    byte[] buffer = new byte[1024];
                    int Size = 0;
                    using (var fstream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), arguments[2], ctx.Parameter)))
                    {
                        fstream.Read(buffer, 0, buffer.Length);
                        Size = (int)fstream.Length;
                    }
                    var content = Encoding.UTF8.GetString(buffer);

                    return server.PrepareOkResponse(ContentType: "application/octet-stream", Length: Size, Body: content);
                }
                else
                {
                    return server.NotFoundResponse;
                }
            });

            server.MapPost("/files", ctx =>
            {
                var arguments = Environment.GetCommandLineArgs();
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), arguments[2], ctx.Parameter), ctx.Body);

                return server.CreatedResponse;
            });

            server.Run();
        }
    }
}

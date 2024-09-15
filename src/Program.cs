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
            var arguments = Environment.GetCommandLineArgs();
            Server server = new Server();

            server.MapGet("/", body => server.PrepareOkResponse() );

            server.MapGet("/user-agent", ctx => server.PrepareOkResponse(ContentType: "text/plain", 
                Length: ctx.Headers["User-Agent"].Length, 
                Body: ctx.Headers["User-Agent"]));

            server.MapGet("/echo", ctx =>
            {
                var str = ctx.Path.Split('/')[^1];
                var compression = ctx.Headers.TryGetValue("Accept-Encoding", out string? encoding);
                return encoding is null ? server.PrepareOkResponse(ContentType: "text/plain",
                    Length: str.Length,
                    Body: str) : server.PrepareOkResponse(ContentType: "text/plain",
                    Length: str.Length,
                    Body: str, Encoding: encoding);
            });

            server.MapGet("/files", ctx =>
            {
                var file = ctx.Path.Split('/')[^1];

                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), args[2], file)))
                {
                    byte[] buffer = new byte[1024];
                    int Size = 0;
                    using (var fstream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), args[2], file)))
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
                var file = ctx.Path.Split('/')[^1];

                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), args[2], file), ctx.Body);

                return server.CreatedResponse;
            });

            server.Run();
        }
    }
}

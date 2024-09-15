using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

            server.MapGet("/", body => server.PrepareOkResponse());

            server.MapGet("/user-agent", ctx => server.PrepareOkResponse(ContentType: "text/plain",
                Length: ctx.Headers["User-Agent"].Length,
                Body: ctx.Headers["User-Agent"]));

            server.MapGet("/echo", ctx =>
            {
                var arguments = Environment.GetCommandLineArgs();
                Console.WriteLine(string.Join("\n", arguments));
                var _ = ctx.Headers.TryGetValue("Accept-Encoding", out string? encoding);

                return encoding is null ? server.PrepareOkResponse(ContentType: "text/plain",
                    Length: ctx.Parameter.Length,
                    Body: ctx.Parameter) : server.PrepareOkResponse(ContentType: "text/plain",
                    Length: Compress(ctx.Parameter).Length,
                    body: Compress(ctx.Parameter), Encoding: encoding);
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

        public static byte[] Compress(string input)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(input);
            byte[] compressed = Compress(encoded);
            return compressed;
        }

        public static byte[] Compress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                using (var result = new MemoryStream())
                {
                    using (var Compress = new GZipStream(result, CompressionMode.Compress))
                    {
                        source.CopyTo(Compress);
                    }

                    return result.ToArray();
                }
            }
        }
    }
}

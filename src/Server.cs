using codecrafters_http_server;
using System.Drawing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using HttpContext = codecrafters_http_server.HttpContext;
using File = System.IO.File;

namespace codecrafters_http_server
{
    public class Server
    {
        int port;
        byte[] arr;
        public string NotFoundResponse = "HTTP/1.1 404 Not Found\r\n\r\n";
        public string PrepareOkResponse(string? ContentType = null, int? Length = null, string? Body = null, byte[]? body = null, string? Encoding = null)
        {
            arr = body;
            return Body is null && body is null ?
            "HTTP/1.1 200 OK\r\n\r\n" : Encoding == null ?
            $"HTTP/1.1 200 OK\r\nContent-Type: {ContentType}\r\nContent-Length: {Length}\r\n\r\n{Body}"
            : $"HTTP/1.1 200 OK\r\nContent-Type: {ContentType}\r\nContent-Encoding: {Encoding}\r\nContent-Length: {Length}\r\n\r\n";
        }

        public string CreatedResponse = "HTTP/1.1 201 Created\r\n\r\n";

        public Server(int port)
        {
            this.port = port;
        }

        public Server() : this(4221) { }

        public void Run()
        {
            Console.WriteLine("Logs from your program will appear here!");

            TcpListener server = new TcpListener(IPAddress.Any, 4221);
            var serverThread = new Thread(new ThreadStart(() =>
            {
                server.Start();

                while (true)
                {
                    var client = server.AcceptTcpClient();

                    var clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
                    clientThread.Start(client);
                }
            }));

            serverThread.Start();
        }


        async void HandleClientCommunication(object tcpClient)
        {
            var client = (TcpClient)tcpClient;
            var response = "";
            var stream = client.GetStream();

            var writer = new StreamWriter(stream);

            string line;

            var buffer = new byte[1024];
            stream.ReadTimeout = 1000;
            List<byte> requestData = new List<byte>();

            try
            {
                int bytesRead;
                do
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        requestData.AddRange(new ArraySegment<byte>(buffer, 0, bytesRead));
                    }
                } while (bytesRead > 0 && !requestData.Contains((byte)'\n'));
            }
            catch (IOException)
            {
                Console.WriteLine("Stream Timeout!");
                if (requestData.Count == 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from stream: {ex.Message}");
                return;
            }

            line = Encoding.UTF8.GetString(requestData.ToArray());


            var request = line.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var ctx = HttpContext.ParseRequest(line);
            //Log Request
            Console.WriteLine(String.Join("\n", request));

            try
            {
                response = RouteHandler.RouteRequest(ctx);
            }
            catch (Exception ex)
            {
                response = NotFoundResponse;
            }

            writer.AutoFlush = true;
            //writer.NewLine = "\r\n";

            if (arr is not null)
            {
                byte[] bresponse = [.. Encoding.UTF8.GetBytes(response), .. arr];
                await stream.WriteAsync(bresponse, 0, bresponse.Length);
                stream.Flush();
            }
            else
            {
                await writer.WriteLineAsync(response);
            }

            client.Close();
        }

        public void MapGet(string Path, Func<HttpContext, string> Action) => RouteHandler.RegisterRoute(Path, "GET", Action);
        public void MapPost(string Path, Func<HttpContext, string> Action) => RouteHandler.RegisterRoute(Path, "POST", Action);
    }
}
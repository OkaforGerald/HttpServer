using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

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

void HandleClientCommunication(object tcpClient)
{
    var client = (TcpClient)tcpClient;
    var response = "";
    var stream = client.GetStream();
    var reader = new StreamReader(stream);
    var writer = new StreamWriter(stream);

    var request = new List<string>();
    string line;
    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
    {
        request.Add(line);
    }

    var tokens = request[0].Split(' ');
    var path = tokens[1];

    if (path == "/")
    {
        response = "HTTP/1.1 200 OK\r\n\r\n";
    }
    else if (path.StartsWith("/echo"))
    {
        var str = path.Split('/')[^1];
        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {str.Length}\r\n\r\n{str}";
    }else if (path.StartsWith("/files"))
    {
        var file = path.Split('/')[1];
        if (File.Exists($"{Directory.GetCurrentDirectory}/tmp/{file}"))
        {
            byte[] buffer = new byte[1024];
            var info = new FileInfo($"{Directory.GetCurrentDirectory}/tmp/{file}");
            using(var fstream = info.OpenRead())
            {
                fstream.Write(buffer, 0, buffer.Length);
            }

            var content = Encoding.UTF8.GetString(buffer);

            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {info.Length}\r\n\r\n{content}";
        }
        else
        {
            response = "HTTP/1.1 404 Not Found\r\n\r\n";
        }
    }
    else if (path.StartsWith("/user-agent"))
    {
        var header = request.FirstOrDefault(x => x.StartsWith("User-Agent:"));
        var value = header.Split(':')[1].Trim();
        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {value.Length}\r\n\r\n{value}";
    }
    else
    {
        response = "HTTP/1.1 404 Not Found\r\n\r\n";
    }

    writer.AutoFlush = true;
    writer.NewLine = "\r\n";

    writer.WriteLine(response);

    stream.Close();
}

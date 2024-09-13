using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

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
    else if (path.Contains("/echo/"))
    {
        var str = path.Split('/')[^1];
        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {str.Length}\r\n\r\n{str}";
    }
    else if (path.Contains("/user-agent"))
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

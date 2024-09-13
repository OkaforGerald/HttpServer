using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var client = server.AcceptTcpClient();
var response = "";
var stream = client.GetStream();
var writer = new StreamWriter(stream);

byte[] buffer = new byte[1];
var memStream = new MemoryStream();

while (stream != null && await stream.ReadAsync(buffer, 0, buffer.Length) == 1)
{
    switch (buffer[0])
    {
        case (byte)'\n':
            stream = null;
            break;

        case (byte)'\r':
            break;

        default:
            memStream.WriteByte(buffer[0]);
            break;
    }
}

var request = Encoding.UTF8.GetString(memStream.ToArray());

var tokens = request.Split(' ');
var path = tokens[1];

if(path == "/")
{
    response = "HTTP/1.1 404 Not Found\r\n\r\n";
}
else if(path.Contains("/echo/"))
{
    var str = path.Split('/')[^1];
    response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {str.Length}\r\n\r\n{str}";
}
else
{
    response = "HTTP/1.1 200 OK\r\n\r\n";
}

writer.AutoFlush = true;
writer.NewLine = "\r\n";

writer.WriteLine(response);

// stream.Close();
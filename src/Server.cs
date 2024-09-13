using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

var client = server.AcceptTcpClient();
var response = "";
var stream = client.GetStream();
byte[] buffer = new byte[1024];

while (stream != null)
{
    await stream.ReadAsync(buffer, 0, buffer.Length);
}

var request = Encoding.UTF8.GetString(buffer);

var tokens = request.Split(' ');
var path = tokens[1];

if(path != "/")
{
    response = "HTTP/1.1 404 Not Found\r\n\r\n";
}
else
{
    response = "HTTP/1.1 200 OK\r\n\r\n";
}

var writer = new StreamWriter(stream);
writer.AutoFlush = true;
writer.NewLine = "\r\n";

writer.WriteLine(response);

stream.Close();



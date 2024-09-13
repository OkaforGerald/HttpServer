using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var client = server.AcceptTcpClient();
var stream = client.GetStream();


var writer = new StreamWriter(stream);
writer.AutoFlush = true;
writer.NewLine = "\r\n";

writer.WriteLine("HTTP/1.1 200 OK\r\n\r\n");



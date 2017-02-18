using System;
using System.Text;
using System.Threading;

namespace WebsocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var socket = new System.Net.WebSockets.ClientWebSocket();
            var token = new CancellationToken(false);
            socket.ConnectAsync(new Uri("ws://localhost:50000/testsample/test3/test2"),token).Wait();
            socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("heelo")),System.Net.WebSockets.WebSocketMessageType.Binary,false,token).Wait();
            var buffer = new ArraySegment<byte>(new byte[1024]);
            var result = socket.ReceiveAsync(buffer, token);
            result.Wait();
            Console.WriteLine(Encoding.UTF8.GetString(buffer.Array,0, result.Result.Count));
            Console.ReadLine();
        }
    }
}
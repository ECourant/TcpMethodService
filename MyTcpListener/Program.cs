using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
class MyTcpListener
{
    public static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);
    public static void Main()
    {
        Console.WriteLine("Creating Server!");
        TcpMethodServer.MethodServer Server = new TcpMethodServer.MethodServer(41000);
        Server.ClientAuthenticating = new TcpMethodServer.AuthenticationHandler(Authenticate);
        Console.WriteLine("Starting Server!");
        Server.Start();
        Console.WriteLine("Server Is Running!");
        Console.ReadLine();
        Server.Stop();
        Console.WriteLine("Server Has Been Shutdown");
        Console.ReadLine();
    }

    public static bool Authenticate(object AuthenticationObject)
    {
        string Name = (string)AuthenticationObject;
        Console.WriteLine($"USER: {Name} IS ATTEMPTING TO AUTHENTICATE!");
        return true;
    }
}
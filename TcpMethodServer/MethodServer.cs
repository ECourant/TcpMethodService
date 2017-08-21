using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
namespace TcpMethodServer
{
    public sealed class MethodServer
    {
        private Guid ServerID { get; set; }
        private int Port { get; set; }
        private TcpListener TcpServer { get; set; }
        private Dictionary<int, ManualResetEvent> ClientResetEvents { get; set; }
        private Dictionary<Guid, TcpClient> ConnectedClients { get; set; }


        private bool IsRunning { get; set; }

        private Thread ListenThread { get; set; }
        public MethodServer(int Port)
        {
            this.ServerID = Guid.NewGuid();
            this.Port = Port;
            this.IsRunning = false;
        }



        public void Start()
        {
            this.ListenThread = new Thread(() => Listener());
            this.ListenThread.IsBackground = true;
            this.ListenThread.Priority = ThreadPriority.Normal;
            this.ListenThread.Start();
            while (!IsRunning)
                Thread.Sleep(100);
        }
        public void Stop()
        {
            try
            {
                this.ListenThread.Abort();
            }
            catch(Exception e)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventHandler ClientConnected;
        /// <summary>
        /// 
        /// </summary>
        public EventHandler ClientDisconnected;
        /// <summary>
        /// 
        /// </summary>
        public AuthenticationHandler ClientAuthenticating { get; set; }



        private void Listener()
        {
            this.IsRunning = false;
#if DEBUG
            Console.WriteLine($"Creating Server On Port [{this.Port}]...");
#endif
            IPAddress LocalAddress = IPAddress.Parse("127.0.0.1");
            TcpServer = new TcpListener(LocalAddress, this.Port);
            TcpServer.Start();
            this.ConnectedClients = new Dictionary<Guid, TcpClient>();
            this.ClientResetEvents = new Dictionary<int, ManualResetEvent>();
            for (int Thread = 0; Thread < Environment.ProcessorCount; Thread++)
            {
                if (!ClientResetEvents.ContainsKey(Thread))
                    ClientResetEvents.Add(Thread, new ManualResetEvent(false));
            }
#if DEBUG
            Console.WriteLine("Waiting For Connection...");
#endif
            this.IsRunning = true;
            Parallel.For(0, Environment.ProcessorCount, ThreadID =>
            {
                while (true)
                {
                    ClientResetEvents[ThreadID].Reset();
                    TcpServer.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClient), new object[] { ThreadID, TcpServer });
                    ClientResetEvents[ThreadID].WaitOne();
                }
            });
        }

        public bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public void AcceptTcpClient(IAsyncResult ar)
        {
            int ThreadID = (int)((object[])ar.AsyncState)[0];
            TcpListener listener = (TcpListener)((object[])ar.AsyncState)[1];
            string Address = string.Empty;
            Guid ClientID = Guid.NewGuid();
            NetworkStream Stream = default(NetworkStream);
            try
            {
                if (ConnectedClients.ContainsKey(ClientID))
                    throw new InvalidOperationException("Error, duplicate client id");
                else
                    ConnectedClients.Add(ClientID, listener.EndAcceptTcpClient(ar));
                Address = ConnectedClients[ClientID].Client.RemoteEndPoint.ToString();
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{Address}] Interogating!");
                Console.ForegroundColor = ConsoleColor.Gray;
#endif

                byte[] bytes = new byte[Defaults.Buffer];
                string data = string.Empty;

                // Get a stream object for reading and writing
                Stream = ConnectedClients[ClientID].GetStream();
                
                ClientResetEvents[ThreadID].Set();
                Listen:
                Console.WriteLine($"[{Address}] Waiting For Request");

                while (true)
                {
                    Task Wait = new Task(() =>
                    {
                        while (!Stream.DataAvailable)
                            Thread.Sleep(1);
                    });
                    Wait.Start();
                    if (!Wait.Wait(TimeSpan.FromSeconds(30)))
                    {
                        try
                        {
                            byte[] Heartbeat = Encoding.ASCII.GetBytes("!@##@!");
                            Stream.WriteAsync(Heartbeat, 0, Heartbeat.Length);
                        }
                        catch (Exception e)
                        {
                            goto DropConnection;
                        }
                    }
                    else
                        break;
                }
                int i;

                Task ReadData = new Task(() =>
                {
                    while (!data.Trim().EndsWith("!@##@!"))
                    {
                        i = Stream.Read(bytes, 0, bytes.Length);
                        data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    }
                });
                ReadData.Start();
                if (ReadData.Wait(TimeSpan.FromMinutes(5)))
                    Console.WriteLine($"[{Address}] Finished Reading Stream!");
                else
                    Console.WriteLine($"[{Address}] Stream Timed Out!");
                data = data.Replace("!@##@!", string.Empty);
                if (!string.IsNullOrWhiteSpace(data))
                {
                    MRequest Request = ToRequest(data);
                    data = string.Empty;
                    Stream.Flush();
                    Request.Address = Address;
                    Request.ClientID = ClientID;
                    try
                    {
                        byte[] ResponseBytes;
                        switch (Request.RequestType)
                        {
                            case ERequestType.Connect:
#if DEBUG
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"[{Address}] Requesting Connection!");
                                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                                if (this.ClientAuthenticating != null)
                                {
                                    try
                                    {
                                        if (!this.ClientAuthenticating(Request.Headers[Defaults.AuthorizationHeader]))
                                            throw new AccessViolationException("Invalid authorization credentials provided!");
                                    }
                                    catch (AccessViolationException e)
                                    {
                                        throw new AccessViolationException(e.Message);
                                    }
                                }
                                else
                                {
#if DEBUG
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"ALERT: Client [{Address}] is connecting but will not be authenticated because there is no authentication method defined!");
                                    Console.ForegroundColor = ConsoleColor.Gray;
#endif
                                }
                                MResponse ConnectionSuccessfulResponse = new MResponse();
                                ConnectionSuccessfulResponse.ClientID = ClientID;
                                ConnectionSuccessfulResponse.ServerID = ServerID;
                                ConnectionSuccessfulResponse.RequestID = Request.RequestID;
                                ConnectionSuccessfulResponse.Response = true;
                                ResponseBytes = ToResponse(ConnectionSuccessfulResponse);
                                Stream.WriteAsync(ResponseBytes, 0, ResponseBytes.Length);
#if DEBUG
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"[{Address}] Has Successfully Connected!");
                                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                                if (ClientConnected != null)
                                    this.ClientConnected(ClientID, EventArgs.Empty);
                                break;
                            case ERequestType.Disconnect:

                                break;
                            case ERequestType.GetInfo:
#if DEBUG
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine($"[{Address}] Requesting Information!");
                                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                                MResponse GetInfoResponse = new MResponse();
                                GetInfoResponse.ClientID = ClientID;
                                GetInfoResponse.ServerID = ServerID;
                                GetInfoResponse.RequestID = Request.RequestID;
                                GetInfoResponse.Response = Environment.MachineName;
                                ResponseBytes = ToResponse(GetInfoResponse);
                                Stream.WriteAsync(ResponseBytes, 0, ResponseBytes.Length);
#if DEBUG
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"[{Address}] Returning Information!");
                                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                                break;
                            case ERequestType.Invoke:
#if DEBUG
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"[{Address}] Requested Service: {Request.Service} Method: {Request.Method} RequestID: {Request.RequestID}");
                                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                                MResponse Response = GetResponse(Request);
                                ResponseBytes = ToResponse(Response);
                                Stream.WriteAsync(ResponseBytes, 0, ResponseBytes.Length);
                                break;
                            case ERequestType.SetBuffer:

                                break;
                        }
                    }
                    catch (AccessViolationException e)
                    {
                        MResponse AccessViolationResponse = new MResponse();
                        AccessViolationResponse.ClientID = ClientID;
                        AccessViolationResponse.RequestID = Request.RequestID;
                        AccessViolationResponse.ServerID = ServerID;
                        AccessViolationResponse.Response = null;
                        AccessViolationResponse.Exception = new AccessViolationException(e.Message);
                        byte[] ResponseBytes = ToResponse(AccessViolationResponse);
                        Stream.WriteAsync(ResponseBytes, 0, ResponseBytes.Length);
#if DEBUG
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{Address}] Provided Invalid Credentials And Will Be Disconnected Message: {e.Message}");
                        Console.ForegroundColor = ConsoleColor.Gray;
#endif
                        ConnectedClients[ClientID].Close();
                        goto DropConnection;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine($"[{Address}] Received Heartbeat!");
                }
                goto Listen;
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e);
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{Address}] Disconnected");
                Console.ForegroundColor = ConsoleColor.Gray;
#endif
            }
            DropConnection:
            Stream.Close();
            if (ConnectedClients.ContainsKey(ClientID))
            {
                ConnectedClients[ClientID].Client.Close();
                ConnectedClients[ClientID].Close();
                ConnectedClients.Remove(ClientID);
            }
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{Address}] Dropped Connection");
            Console.ForegroundColor = ConsoleColor.Gray;
#endif
            if (ClientDisconnected != null)
                this.ClientDisconnected(ClientID, EventArgs.Empty);
            
        }

        private static MRequest ToRequest(string Data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MRequest>(Data);
        }
        private static byte[] ToResponse(MResponse Response)
        {
            return Encoding.ASCII.GetBytes($"{Newtonsoft.Json.JsonConvert.SerializeObject(Response)}!@##@!");
        }
        private MResponse GetResponse(MRequest Request)
        {
            
            return null;
        }
    }
}

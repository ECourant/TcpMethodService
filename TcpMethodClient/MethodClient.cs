using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace TcpMethodClient
{
    public abstract class MethodClient
    {

        private Dictionary<string, Dictionary<string, MethodInfo>> Methods { get; set; }





        private TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }
        private Thread ListenThread { get; set; }
        private bool EndConnection { get; set; }

        internal MethodClient()
        {
            throw new InvalidOperationException("MethodClient cannot be initialized without a server Address and Port");
        }
        public MethodClient(string Address, int Port)
        {
            this.EndConnection = false;
            this.Client = new TcpClient(Address, Port);
            this.Stream = Client.GetStream();
            this.ListenThread = new Thread(() => _ReceiveResponse());
            this.ListenThread.IsBackground = true;
            this.ListenThread.Priority = ThreadPriority.Normal;
            this.ListenThread.Start();

        }
        public object Credentials { get; set; }



        private MResponse ClientInvoke(MRequest Request)
        {
            this.RequestQueue.Enqueue(Request);
            _TransmitRequest(Request);
            return null;
        }

        protected void Initialize()
        {
            this.RequestQueue = new RequestQueue<MRequest>();
            this.RequestQueue.Enqueued = this.RequestQueued;
            Console.WriteLine($"Initiating New [{this.GetType().Name}]");
            foreach (var ServiceClass in this.GetType().GetFields())
            {
                Console.WriteLine($"Found [{ServiceClass.Name}]");

                MethodInfo Bind = ServiceClass.GetValue(this).GetType().GetMethod("_Bind", 
                    System.Reflection.BindingFlags.CreateInstance | 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.InvokeMethod | 
                    System.Reflection.BindingFlags.NonPublic);
                Console.WriteLine($"Found Binding Method For Type [{ServiceClass.Name}], Binding Now!");
                try
                {
                    Bind.Invoke(ServiceClass.GetValue(this), new object[] { (MethodInvokeHandler)this.ClientInvoke });
                    Console.WriteLine($"Successfully Bound MethodService [{ServiceClass.Name}]");
                }
                catch(Exception e)
                {
                    throw new Exception($"Error, could not bind client to MethodService [{ServiceClass.Name}]");
                }
                foreach (var Method in ServiceClass.GetValue(this).GetType().GetMethods())
                {
                    TcpMethod MethodAttribute = Method.GetCustomAttribute<TcpMethod>();
                    if (MethodAttribute != null)
                    {
                        Console.WriteLine($"Found Method [{Method.Name}]");
                    }
                }
            }
        }

        private void _TransmitRequest(MRequest Request)
        {
            try
            {
                if (Stream.CanWrite)
                {
                    byte[] RequestData = ToRequest(Request);
                    Client.GetStream().WriteAsync(RequestData, 0, RequestData.Length);
                }
                else
                    Console.WriteLine("ERROR");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }



        private void _ReceiveResponse()
        {
            try
            {
                while (true)
                {
                    Task WaitForData = new Task(() =>
                    {
                        while (!Stream.DataAvailable && !EndConnection)
                            Thread.Sleep(1);
                    });
                    WaitForData.Start();
                    if (WaitForData.Wait(TimeSpan.FromSeconds(60)))
                    {
                        Console.WriteLine("Receiving Data!");
                        byte[] bytes = new byte[256];
                        string ResponseData = string.Empty;
                        int i;
                        while (!ResponseData.EndsWith(Defaults.Signature))
                        {
                            i = Stream.Read(bytes, 0, bytes.Length);
                            ResponseData += Encoding.ASCII.GetString(bytes, 0, i);
                        }
                        Task.Run(() => ProcessResponse(ResponseData));
                        Stream.Flush();
                    }
                    else
                    {
                        try
                        {
                            byte[] Heartbeat = Encoding.ASCII.GetBytes("!@##@!");
                            Stream.WriteAsync(Heartbeat, 0, Heartbeat.Length);
                        }
                        catch (Exception e)
                        {
                            goto Disconnect;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            Disconnect:
            this.Stream.Close();
            this.Client.Close();
            Console.WriteLine("Disconnected!");
        }

        private void ProcessResponse(string Data)
        {
            if (Data == Defaults.Signature)
                Console.WriteLine("Received Heartbeat From Server!");
            else
            {
                Console.WriteLine("Received Data!");
                Console.WriteLine(Data);
            }
        }

        private void Connect()
        {
            //Guid ClientID = this.ClientInvoke("#MethodService", "Connect", null);
            MRequest ConnectRequest = new MRequest();
            ConnectRequest.RequestID = Guid.NewGuid();
            ConnectRequest.ClientID = default(Guid);
            ConnectRequest.Arguments = null;
            ConnectRequest.Headers = new Dictionary<string, string>() { { "Authorization", "Elliot" } };
            ConnectRequest.Method = "#Connect";
            ConnectRequest.Service = "#Server";
            ConnectRequest.RequestType = ERequestType.Connect;
            _TransmitRequest(ConnectRequest);
        }
        public void Disconnect()
        {
            MRequest ConnectRequest = new MRequest();
            ConnectRequest.RequestID = Guid.NewGuid();
            ConnectRequest.ClientID = default(Guid);
            ConnectRequest.Arguments = null;
            ConnectRequest.Method = "#Disconnect";
            ConnectRequest.Service = "#Server";
            ConnectRequest.RequestType = ERequestType.Disconnect;
            _TransmitRequest(ConnectRequest);
        }
        public void ForceDisconnect()
        {
            try
            {
                this.EndConnection = true;
            }
            catch(Exception e)
            {

            }
            this.Stream.Close();
            this.Client.Close();
            Console.WriteLine("Disconnected!");
        }
        public void GetInfo()
        {
            MRequest ConnectRequest = new MRequest();
            ConnectRequest.RequestID = Guid.NewGuid();
            ConnectRequest.ClientID = default(Guid);
            ConnectRequest.Arguments = null;
            ConnectRequest.Method = "#GetInfo";
            ConnectRequest.Service = "#Server";
            ConnectRequest.RequestType = ERequestType.GetInfo;
            _TransmitRequest(ConnectRequest);
        }

        private RequestQueue<MRequest> RequestQueue { get; set; }

        private void RequestQueued(object sender, EventArgs e)
        {

        }


        private byte[] ToRequest(MRequest Request)
        {
            return System.Text.Encoding.ASCII.GetBytes($"{Newtonsoft.Json.JsonConvert.SerializeObject(Request)}!@##@!");
        }
    }
}

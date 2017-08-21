using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace MyTcpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating Client!");
            ClientClass Client = new ClientClass("127.0.0.1", 41000);
            Console.WriteLine("Client Created!");
            Console.ReadLine();
            Client.Inventory.AddInventory("Test", 2);
            Console.ReadLine();
            
            Client.ForceDisconnect();
        }


        public class ClientClass : TcpMethodClient.MethodClient
        {
            public ClientClass(string Address, int Port) : base(Address, Port)
            {
                Inventory = new Inventory();
                Initialize();
            }

            public Inventory Inventory;
        }

        public class Inventory : TcpMethodClient.MethodService
        {
            [TcpMethodClient.TcpMethod]
            public void AddInventory(string SKU, int QTY)
            {
                this.Invoke(SKU, QTY);
            }

            [TcpMethodClient.TcpMethod]
            public void RemoveInventory(string SKU, int QTY)
            {
                this.Invoke(SKU, QTY);
            }
        }
    }
}

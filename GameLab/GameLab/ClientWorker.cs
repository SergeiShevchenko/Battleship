using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace GameLab
{
    class ClientWorker
    {
        int port = 1488;

        GameController GC;

        public ClientWorker(GameController gc)
        {
            GC = gc;
            threads = new List<Thread>();
        }

        // Сlient socket
        public Socket client;

        public bool connected;

        public void Connect(string ipString)
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(IPAddress.Parse(ipString), port);
                connected = true;
                Receive();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<Thread> threads;
        public bool work = true;

        void Receive()
        {
            Thread th = new Thread(delegate()
            {
                byte[] bytes = new byte[1024];
                while (work)
                {
                        // Receives data from server
                        int messageSize = client.Receive(bytes);
                        string data = Encoding.Unicode.GetString(bytes, 0, messageSize);                            
                        GC.ProcessIncoming(data);
                }
            });
            // Start the thread
            threads.Add(th);
            th.Start();
        }


        // Send info to server
        public void Send(string msg)
        {
            client.Send(Encoding.Unicode.GetBytes(msg + "|"));
        }  

    }
}

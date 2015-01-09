using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace GameLabServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        // Server socket
        Socket listener;

        // Which port we are goint to use
        int port = 1488;

        // Point for incoming messages
        IPEndPoint Point;

        List<Thread> threads;


        // Starting of server
        public void ServerStart()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Determining an endpoint, IPAddress.Any means we will accept connections from any IP adresses
            Point = new IPEndPoint(IPAddress.Any, port);
            // Binding server socket with endpoint
            listener.Bind(Point);
            // Start listening for incoming connections
            listener.Listen(10);
            threads = new List<Thread>();
            SocketAccepter();
        }

        List<Socket> clients;

        public IPAddress GetMyIP()
        {
            IPHostEntry host;
            IPAddress localIP = IPAddress.Parse("0.0.0.0");
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip;
                }
            }
            return localIP;
        }

        bool work = true;

        private void SocketAccepter()
        {

                clients = new List<Socket>();
                Thread th = new Thread(delegate()
                {
                    while (work)
                    {
                        if (listener.IsBound)
                        {
                            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            try
                            { 
                                client = listener.Accept(); 
                            }
                            catch
                            {
                                return;
                            }

                            // Adding a new player to the game
                            //Players.Add(Newbie);

                            // For every new client, new thread
                            Thread thh = new Thread(delegate()
                            {
                                byte[] bytes = new byte[1024];
                                while (true)
                                {
                                    try
                                    {
                                        // Receive stuff
                                        int messageSize = client.Receive(bytes);
                                        // Convert it into a readable string
                                        string data = Encoding.Unicode.GetString(bytes, 0, messageSize);
                                        textBox1.Invoke(new Action(() => textBox1.Text += Environment.NewLine + data + " от " + client.RemoteEndPoint.ToString()));
                                        // Process it
                                        ProcessIncomingInfo(data, client);
                                    }
                                    catch (Exception e)
                                    {
                                        //MessageBox.Show("Message receive failed: "+e.Message);
                                    }
                                }
                            }
                        );
                            threads.Add(thh);
                            thh.Start();
                        }
                    }
                });
                threads.Add(th);
                th.Start();            
        }

        private void MessageSender(Socket c_client, string s)
        {
            try
            {
                byte[] bytes = new byte[1024];
                bytes = Encoding.Unicode.GetBytes(s + "*");
                c_client.Send(bytes);
                textBox1.Invoke(new Action(() => textBox1.Text += Environment.NewLine + s + " к " + c_client.RemoteEndPoint.ToString()));
            }
            catch (Exception e)
            {
                //MessageBox.Show("Message sending failed: " + e.Message);
            }
        }
        int readyCount = 0;
        public void ProcessIncomingInfo(string s, Socket p)
        {
            string[] tokens = s.Split('*');
            switch (tokens[0])
            {
                case "READY":
                    {
                        if (!clients.Contains(p))
                        {
                            clients.Add(p);
                            readyCount++;
                            if (readyCount == 2)
                            {
                                MessageSender(clients[0], "GAMEON*TRUE");
                                MessageSender(clients[1], "GAMEON*FALSE");
                            }
                        }
                        break;
                    }
                case "HIT":
                    {
                        MessageSender(clients.Find(sck => sck != p), s);
                        break;
                    }
                case "MISS":
                    {
                        MessageSender(clients.Find(sck => sck != p), s);
                        break;
                    }
                case "FIRE":
                    {
                        MessageSender(clients.Find(sck => sck != p), s);
                        break;
                    }
                case "BOMBED":
                    {
                        MessageSender(clients.Find(sck => sck != p), s);
                        break;
                    }
                case "LOST":
                    {
                        MessageSender(clients.Find(sck => sck != p), s);
                        readyCount = 0;
                        MessageSender(clients[0], "GAMEOFF");
                        MessageSender(clients[1], "GAMEOFF");
                        clients.Clear();
                        break;
                    }
            }

            return;
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
            ServerStart();
            IPAddress myIp = GetMyIP();
            textBox1.Text = myIp.ToString();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (listener != null) listener.Close();
            work = false;
            foreach (Thread t in threads)
                t.Abort();
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

    }
}

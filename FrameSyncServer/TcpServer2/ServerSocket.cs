using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer2
{
    internal class ServerSocket
    {
        //服务器端的socket
        public Socket socket;
        //客户端连接的所有的socket
        public Dictionary<int, ClientSocket> ClientSockets = new Dictionary<int, ClientSocket>();
        private bool isClose;

        //开启服务器
        public void Start(string ip, int port, int num)
        {
            isClose = false;
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(ipPoint);
            socket.Listen(num);

            ThreadPool.QueueUserWorkItem(Accept);
            ThreadPool.QueueUserWorkItem(Receive);
        }

        //关闭服务器
        public void Close()
        {
            isClose = true;
            foreach (ClientSocket client in ClientSockets.Values)
            {
                client.Close();
            }
            ClientSockets.Clear();

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }


        //接受客户端连入
        private void Accept(object obj)
        {
            while (!isClose)
            {
                try
                {
                    Socket clientSocket = socket.Accept();
                    ClientSocket client = new ClientSocket(clientSocket);
                    client.Send("欢迎连入服务器");
                    ClientSockets.Add(client.clientId, client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        //接受客户端消息
        private void Receive(object obj)
        {
            while(!isClose)
            {
                if (ClientSockets.Count > 0)
                {
                    foreach (ClientSocket client in ClientSockets.Values)
                    {
                        client.Receive();
                    }
                }
            }
        }

        //广播
        public void Broadcast(string info)
        {
            foreach (ClientSocket client in ClientSockets.Values)
            {
                client.Send(info);
            }
        }
    }
}

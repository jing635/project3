using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TcpServer2
{
    internal class ClientSocket
    {
        public Socket socket;
        public int clientId;
        private static int Client_Begin_Id = 1;

        public ClientSocket(Socket socket)
        {
            this.clientId = Client_Begin_Id;
            this.socket = socket;
            ++Client_Begin_Id;
        }

        //是否是连接状态
        public bool IsConnect => this.socket.Connected;

        //关闭
        public void Close()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }

        //发送
        public void Send(string info)
        {
            if (socket !=null)
            {
                try
                {
                    socket.Send(Encoding.UTF8.GetBytes(info));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
        }

        //接受
        public void Receive()
        {
            if (socket !=null)
            {
                return;
            }

            try
            {
                if (socket.Available > 0)
                {
                    byte[] result = new byte[1024 * 5];
                    int receiveNum = socket.Receive(result);
                    ThreadPool.QueueUserWorkItem(MsgHandle,(Encoding.UTF8.GetString(result, 0, receiveNum)));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Close();
            }
        }

        private void MsgHandle(object obj)
        {
            string str = obj as string;
            //当前只负责登录验证
        }
    }
}

using MySql.Data.MySqlClient;


namespace TcpServer2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //连上mysql服务器
            string connection = "server=localhost;database=userdata;user=root;password=password;";
            using(MySqlConnection conn = new MySqlConnection(connection))
            {
                conn.Open();

                
            }

            //启动服务器
            ServerSocket socket = new ServerSocket();
            socket.Start("127.0.0.1", 8080, 100);
            while (true)
            {
                string input = Console.ReadLine();
                if(input == "Quit")
                {
                    socket.Close();
                }
                
            }
        }
    }
}

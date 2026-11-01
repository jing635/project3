using MySql.Data.MySqlClient;


namespace TcpServer2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //连上mysql服务器
            string connection = "server=localhost;user=root;password=password;";
            using(MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("成功连上数据库");

                    //创建数据库
                    string createDbSql = "CREATE DATABASE IF NOT EXISTS 'User' CHARACTER SET utf8mb4;";
                    using(MySqlCommand cmd = new MySqlCommand(createDbSql,conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("数据库创建成功或已存在");
                    }

                    //切换到刚建的库
                    conn.ChangeDatabase("User");

                    //创建一张表
                    string createTable = @"
                        CREATE TABLE UserData(
                        Username VARCHAR(10) NOT NULL PRIMARY KEY,
                        Password VARCHAR(10),
                        Loginstatus INT NOT NULL DEFAULT 0
                        );";
                    using(var cmd = new MySqlCommand(createTable,conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("表创建成功");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
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

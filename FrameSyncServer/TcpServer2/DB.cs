using System;
using MySql.Data.MySqlClient;

namespace TcpServer2
{
    internal static class DB
    {
        private const string ConnStr =
            "server=127.0.0.1;port=3306;database=userdata;user=root;password=password;charset=utf8mb4;";

        private static MySqlConnection Open()
        {
            MySqlConnection conn = new MySqlConnection(ConnStr);
            conn.Open();
            return conn;
        }

        public static bool TestConnect()
        {
            try
            {
                using (MySqlConnection c = Open()) { }
                Console.WriteLine("数据库连接成功");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("数据库连接失败：" + e.Message);
                return false;
            }
        }

        // 服务器启动时把所有在线标记刷成离线
        public static void ResetAllOnline()
        {
            using (MySqlConnection conn = Open())
            using (MySqlCommand cmd = new MySqlCommand("UPDATE `user` SET `online` = 0", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        // 登录：0 成功 / 1 用户不存在 / 2 密码错误
        public static int Login(string username, string password, out int userId)
        {
            userId = 0;

            using (MySqlConnection conn = Open())
            using (MySqlCommand cmd = new MySqlCommand(
                "SELECT id, password FROM `user` WHERE username = @u LIMIT 1", conn))
            {
                // 用参数，不要拼字符串，这样别人输 ' or 1=1 也破坏不了 SQL
                cmd.Parameters.AddWithValue("@u", username);

                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return Code.UserNotFound;

                    userId = r.GetInt32("id");
                    string dbPassword = r.GetString("password");

                    return dbPassword == password ? Code.Ok : Code.WrongPassword;
                }
            }
        }

        // 注册：0 成功 / 3 用户名已存在
        public static int Register(string username, string password)
        {
            using (MySqlConnection conn = Open())
            using (MySqlCommand cmd = new MySqlCommand(
                "INSERT INTO `user`(username, password) VALUES(@u, @p)", conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                try
                {
                    cmd.ExecuteNonQuery();
                    return Code.Ok;
                }
                catch (MySqlException e) when (e.Number == 1062)
                {
                    // 1062 = 唯一索引冲突 = 用户名有人用了
                    return Code.UserExists;
                }
            }
        }

        // 更新在线标记
        public static void SetOnline(int userId, int online)
        {
            using (MySqlConnection conn = Open())
            using (MySqlCommand cmd = new MySqlCommand(
                "UPDATE `user` SET `online` = @o WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@o", online);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        // 读：查出这个玩家的设置
        public static SettingData LoadSetting(int userId)
        {
            SettingData d = new SettingData();

            using (MySqlConnection conn = Open())
            using (MySqlCommand cmd = new MySqlCommand(
                "SELECT isplaymusic, isplaysound, music, sound FROM `user` WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", userId);

                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return d;      // 查不到就用默认值，不要崩

                    d.isPlayMusic = Convert.ToInt32(r["isplaymusic"]) != 0;
                    d.isPlaySound = Convert.ToInt32(r["isplaysound"]) != 0;
                    d.music = Convert.ToSingle(r["music"]);
                    d.sound = Convert.ToSingle(r["sound"]);
                }
            }

            return d;
        }

        // 写：更新这个玩家的设置
        public static void SaveSetting(int userId, SettingData d)
        {
            using (MySqlConnection conn = Open())
            using (MySqlCommand cmd = new MySqlCommand(
                "UPDATE `user` SET isplaymusic=@m, isplaysound=@s, music=@mv, sound=@sv WHERE id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@m", d.isPlayMusic ? 1 : 0);
                cmd.Parameters.AddWithValue("@s", d.isPlaySound ? 1 : 0);
                cmd.Parameters.AddWithValue("@mv", d.music);
                cmd.Parameters.AddWithValue("@sv", d.sound);
                cmd.Parameters.AddWithValue("@id", userId);

                cmd.ExecuteNonQuery();
            }
        }
    }

   
    internal class SettingData
    {
        public bool isPlayMusic = true;
        public bool isPlaySound = true;
        public float music = 0.700f;
        public float sound = 0.700f;
    }
}



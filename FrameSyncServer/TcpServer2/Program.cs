using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpServer2
{
    // 一个玩家的会话 = 一条连接 + 他是谁
    internal class Session
    {
        public Socket Socket;
        public int UserId;
        public string UserName;

        public void Send(BaseMsg msg)
        {
            try
            {
                Socket.Send(Frame.Pack(msg));
            }
            catch (Exception e)
            {
                Console.WriteLine("发送失败：" + e.Message);
            }
        }

        public void Close()
        {
            try { Socket.Shutdown(SocketShutdown.Both); } catch { }
            try { Socket.Close(); } catch { }
        }
    }

    internal class Program
    {
        // 谁在线：userId -> 会话。这是"顶号"的唯一权威
        static readonly Dictionary<int, Session> Sessions = new Dictionary<int, Session>();
        static readonly object SessionLock = new object();

        static void Main(string[] args)
        {
            if (!DB.TestConnect())
            {
                Console.WriteLine("数据库连不上，按回车退出");
                Console.ReadLine();
                return;
            }

            DB.ResetAllOnline();

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
            listener.Listen(100);

            Console.WriteLine("服务器已启动，端口 8080，等待连接...");

            while (true)
            {
                Socket client = listener.Accept();
                client.NoDelay = true;
                Console.WriteLine("客户端连入：" + client.RemoteEndPoint);

                Session session = new Session { Socket = client };
                Thread t = new Thread(() => HandleClient(session));
                t.IsBackground = true;
                t.Start();
            }
        }

        static void HandleClient(Session session)
        {
            byte[] buffer = new byte[64 * 1024];
            int len = 0;

            try
            {
                while (true)
                {
                    int n = session.Socket.Receive(buffer, len, buffer.Length - len, SocketFlags.None);
                    if (n <= 0) break;
                    len += n;

                    int msgId;
                    byte[] body;
                    while (Frame.TryUnpack(buffer, ref len, out msgId, out body))
                    {
                        try
                        {
                            HandleMessage(session, msgId, body);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("处理消息出错：" + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("连接异常：" + e.Message);
            }
            finally
            {
                RemoveSession(session);
                try { session.Socket.Close(); } catch { }
                Console.WriteLine("客户端断开：" + session.Socket.RemoteEndPoint);
            }
        }

        static void HandleMessage(Session session, int msgId, byte[] body)
        {
            switch (msgId)
            {
                case MsgId.LoginReq: HandleLogin(session, body); break;
                case MsgId.RegisterReq: HandleRegister(session, body); break;
                case MsgId.SaveSettingReq: HandleSaveSetting(session, body); break;
                default:
                    Console.WriteLine("未知消息号：" + msgId);
                    break;
            }
        }

        static void HandleSaveSetting(Session session, byte[] body)
        {
            if (session.UserId == 0) return;      

            SaveSettingReq req = new SaveSettingReq();
            req.Read(body);

            DB.SaveSetting(session.UserId, new SettingData
            {
                isPlayMusic = req.isPlayMusic,
                isPlaySound = req.isPlaySound,
                music = Math.Clamp(req.music, 0, 1),      // 不信客户端，夹一下
                sound = Math.Clamp(req.sound, 0, 1)
            });

            session.Send(new SaveSettingRes { code = Code.Ok });

            Console.WriteLine("[设置] 用户 " + session.UserName + " 已保存 音乐=" + req.music + " 音效=" + req.sound);
        }

        // ---------------- 登录 + 顶号 ----------------
        static void HandleLogin(Session session, byte[] body)
        {
            LoginReq req = new LoginReq();
            req.Read(body);
            Console.WriteLine("[登录] 用户名=" + req.username);

            int userId;
            int result = DB.Login(req.username, req.password, out userId);

            if (result != Code.Ok)
            {
                session.Send(new LoginRes
                {
                    code = result,
                    userId = 0,
                    message = result == Code.UserNotFound ? "用户名不存在" : "密码错误"
                });
                Console.WriteLine("[登录] 失败：" + req.username);
                return;
            }

            session.UserId = userId;
            session.UserName = req.username;

            // 这条连接之前登录过别人，先把旧会话摘掉
            if (session.UserId != 0 && session.UserId != userId)
            {
                Session oldSession;
                lock (SessionLock)
                {
                    if (Sessions.TryGetValue(session.UserId, out oldSession) && oldSession == session)
                        Sessions.Remove(session.UserId);
                }
                DB.SetOnline(session.UserId, 0);
            }

            // ---- 顶号：先把新会话塞进去，再处理旧的 ----
            Session old;
            lock (SessionLock)
            {
                Sessions.TryGetValue(userId, out old);
                Sessions[userId] = session;
            }

            if (old != null && old != session)
            {
                Console.WriteLine("[顶号] " + req.username + " 在别处登录，踢掉旧连接");
                old.Send(new KickNotify { reason = "你的账号在其他地方登录" });
                old.Close();
            }

            DB.SetOnline(userId, 1);

            SettingData setting = DB.LoadSetting(userId);

            session.Send(new LoginRes
            {
                code = Code.Ok,
                userId = userId,
                message = "登录成功",
                isPlayMusic = setting.isPlayMusic,
                isPlaySound = setting.isPlaySound,
                music = setting.music,
                sound = setting.sound
            });

            Console.WriteLine("[登录] 成功：" + req.username + " userId=" + userId);

        }

        // ---------------- 注册 ----------------
        static void HandleRegister(Session session, byte[] body)
        {
            RegisterReq req = new RegisterReq();
            req.Read(body);
            Console.WriteLine("[注册] 用户名=" + req.username);

            int result = DB.Register(req.username, req.password);

            session.Send(new RegisterRes
            {
                code = result,
                message = result == Code.Ok ? "注册成功" : "用户名已被占用"
            });

            Console.WriteLine("[注册] " + (result == Code.Ok ? "成功" : "失败"));
        }

        // ---------------- 断开清理 ----------------
        static void RemoveSession(Session session)
        {
            if (session.UserId == 0) return;

            lock (SessionLock)
            {
                // 只有"当前生效的会话"断开，才需要置离线。
                // 被顶号的那个旧会话，它的 userId 已经被新会话接管，这里会跳过。
                Session cur;
                if (Sessions.TryGetValue(session.UserId, out cur) && cur == session)
                {
                    Sessions.Remove(session.UserId);
                    DB.SetOnline(session.UserId, 0);
                    Console.WriteLine("[离线] " + session.UserName);
                }
            }
        }
    }
}

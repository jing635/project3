using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpServer3
{
    internal class Session
    {
        public Socket Socket;
        public int UserId;
        public int RoomId;
        public int CharacterId;
        public int PlayerIndex = -1;

        public void Send(BaseMsg msg)
        {
            try { Socket.Send(Frame.Pack(msg)); }
            catch (Exception e) { Console.WriteLine("发送失败：" + e.Message); }
        }

        public void Close()
        {
            try { Socket.Shutdown(SocketShutdown.Both); } catch { }
            try { Socket.Close(); } catch { }
        }
    }

    internal class Room
    {
        public int RoomId;
        public int Seed;
        public List<Session> Players = new List<Session>();

        public bool IsFull { get { return Players.Count >= 2; } }
    }

    internal class Program
    {
        private const int Port = 8081;

        // roomId -> 房间
        private static readonly Dictionary<int, Room> Rooms = new Dictionary<int, Room>();
        private static readonly object RoomLock = new object();

        static void Main(string[] args)
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, Port));
            listener.Listen(100);

            Console.WriteLine("战斗服务器已启动，端口 " + Port + "，等待连接...");

            while (true)
            {
                Socket client = listener.Accept();
                client.NoDelay = true;

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
                        try { HandleMessage(session, msgId, body); }
                        catch (Exception e) { Console.WriteLine("处理消息出错：" + e.Message); }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("连接异常：" + e.Message);
            }
            finally
            {
                LeaveRoom(session);
                try { session.Socket.Close(); } catch { }
            }
        }

        static void HandleMessage(Session session, int msgId, byte[] body)
        {
            switch (msgId)
            {
                case MsgId.JoinBattleReq: HandleJoinBattle(session, body); break;
                default: Console.WriteLine("战斗服收到未知消息号：" + msgId); break;
            }
        }

        static void HandleJoinBattle(Session session, byte[] body)
        {
            JoinBattleReq req = new JoinBattleReq();
            req.Read(body);

            Room room = null;
            Session other = null;
            bool isFull = false;

            lock (RoomLock)
            {
                if (!Rooms.TryGetValue(req.roomId, out room))
                {
                    room = new Room { RoomId = req.roomId, Seed = new Random().Next() };
                    Rooms[req.roomId] = room;
                    Console.WriteLine("[房间 " + req.roomId + "] 创建，随机种子 " + room.Seed);
                }

                if (room.IsFull)
                {
                    isFull = true;
                }
                else
                {
                    session.UserId = req.userId;
                    session.RoomId = req.roomId;
                    session.CharacterId = req.characterId;
                    session.PlayerIndex = room.Players.Count;

                    room.Players.Add(session);

                    if (room.Players.Count == 2) other = room.Players[0];
                }
            }

            if (isFull)
            {
                session.Send(new JoinBattleRes { code = 1, message = "房间已满" });
                return;
            }

            session.Send(new JoinBattleRes { code = 0, message = "已进入房间" });

            Console.WriteLine("[房间 " + req.roomId + "] 玩家 " + req.userId
                + " 进入，角色 " + req.characterId + "，当前 " + (other == null ? 1 : 2) + " 人");

            if (other == null)
            {
                Console.WriteLine("[房间 " + req.roomId + "] 等待对手...");
                return;   // 只有一个人，等对手
            }

            // 两人到齐 → 开局
            Console.WriteLine("[房间 " + req.roomId + "] 双方就位，开始战斗！");

            other.Send(new BattleStartNotify
            {
                roomId = room.RoomId,
                playerIndex = 0,
                myCharacterId = other.CharacterId,
                enemyCharacterId = session.CharacterId,
                randomSeed = room.Seed
            });

            session.Send(new BattleStartNotify
            {
                roomId = room.RoomId,
                playerIndex = 1,
                myCharacterId = session.CharacterId,
                enemyCharacterId = other.CharacterId,
                randomSeed = room.Seed
            });
        }

        static void LeaveRoom(Session session)
        {
            if (session.RoomId == 0) return;

            Session other = null;

            lock (RoomLock)
            {
                Room room;
                if (Rooms.TryGetValue(session.RoomId, out room))
                {
                    room.Players.Remove(session);

                    if (room.Players.Count == 0)
                    {
                        Rooms.Remove(session.RoomId);
                        Console.WriteLine("[房间 " + session.RoomId + "] 已解散");
                        return;
                    }

                    other = room.Players[0];
                }
            }

            if (other != null)
            {
                Console.WriteLine("[房间 " + session.RoomId + "] 对手掉线");
                other.Send(new EnemyLeftNotify { reason = "对手已离开" });
            }
        }
    }
}

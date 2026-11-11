using System.Collections.Generic;

namespace TcpServer2
{
    // 排队中的一个玩家
    internal class MatchPlayer
    {
        public int UserId;
        public string UserName;
        public int CharacterId;
        public Session Session;
    }

    internal static class Matchmaker
    {
        // 战斗服地址，本地开发先写死
        public const string BattleIp = "127.0.0.1";
        public const int BattlePort = 8081;

        private static readonly List<MatchPlayer> Queue = new List<MatchPlayer>();
        private static readonly object Lock = new object();
        private static int nextRoomId = 1;

        // 加入队列。够两个人就地配对，给双方发"匹配成功"
        public static void Enqueue(Session session, int characterId)
        {
            MatchPlayer a = null;
            MatchPlayer b = null;
            int roomId = 0;
            bool alreadyInQueue = false;

            lock (Lock)
            {
                for (int i = 0; i < Queue.Count; i++)
                {
                    if (Queue[i].UserId == session.UserId)
                    {
                        alreadyInQueue = true;
                        break;
                    }
                }

                if (!alreadyInQueue)
                {
                    Queue.Add(new MatchPlayer
                    {
                        UserId = session.UserId,
                        UserName = session.UserName,
                        CharacterId = characterId,
                        Session = session
                    });

                    if (Queue.Count >= 2)
                    {
                        a = Queue[0];
                        b = Queue[1];
                        Queue.RemoveRange(0, 2);
                        roomId = nextRoomId++;
                    }
                }
            }

            if (alreadyInQueue)
            {
                session.Send(new MatchRes { code = MatchCode.Queued, message = "已经在匹配队列里了" });
                return;
            }

            if (a == null)
            {
                session.Send(new MatchRes { code = MatchCode.Queued, message = "正在匹配对手..." });
                System.Console.WriteLine("[匹配] " + session.UserName + " 进入队列，当前 " + Queue.Count + " 人");
                return;
            }

            System.Console.WriteLine("[匹配] 配对成功：房间 " + roomId + "，" + a.UserName + " vs " + b.UserName);

            // 房间号交给双方，各发一次，注意"我方/对方"是反的
            a.Session.Send(new MatchSuccessNotify
            {
                ip = BattleIp,
                port = BattlePort,
                roomId = roomId,
                myCharacterId = a.CharacterId,
                enemyCharacterId = b.CharacterId,
                enemyName = b.UserName
            });

            b.Session.Send(new MatchSuccessNotify
            {
                ip = BattleIp,
                port = BattlePort,
                roomId = roomId,
                myCharacterId = b.CharacterId,
                enemyCharacterId = a.CharacterId,
                enemyName = a.UserName
            });
        }

        // 取消匹配。返回 true 表示成功从队列里拿掉了
        public static void Cancel(Session session)
        {
            bool removed = false;

            lock (Lock)
            {
                for (int i = 0; i < Queue.Count; i++)
                {
                    if (Queue[i].UserId == session.UserId)
                    {
                        Queue.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
            }

            if (removed)
            {
                session.Send(new MatchRes { code = MatchCode.Cancelled, message = "已取消匹配" });
                System.Console.WriteLine("[匹配] " + session.UserName + " 取消匹配");
            }
            else
            {
                session.Send(new MatchRes { code = MatchCode.Fail, message = "你不在匹配队列里" });
            }
        }

        // 玩家断线时把他从队列里摘掉
        public static void Remove(int userId)
        {
            lock (Lock)
            {
                for (int i = 0; i < Queue.Count; i++)
                {
                    if (Queue[i].UserId == userId)
                    {
                        Queue.RemoveAt(i);
                        System.Console.WriteLine("[匹配] 玩家 " + userId + " 断线，移出队列");
                        break;
                    }
                }
            }
        }
    }
}

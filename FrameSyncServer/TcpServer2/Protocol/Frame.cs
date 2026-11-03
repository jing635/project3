using System;

public static class Frame
{
    public static int HeaderSize { get { return 8; } }

    // 打包：消息 → [4字节长度][4字节消息号][内容]
    public static byte[] Pack(BaseMsg msg)
    {
        byte[] body = msg.Write();
        byte[] result = new byte[8 + body.Length];

        Array.Copy(BitConverter.GetBytes(4 + body.Length), 0, result, 0, 4);
        Array.Copy(BitConverter.GetBytes(msg.Id()), 0, result, 4, 4);
        Array.Copy(body, 0, result, 8, body.Length);

        return result;
    }

    // 拆包：缓冲区里有完整的一条就取出来，返回 true
    public static bool TryUnpack(byte[] buffer, ref int length, out int msgId, out byte[] body)
    {
        msgId = 0;
        body = null;

        if (length < 8) return false;

        int bodyLen = BitConverter.ToInt32(buffer, 0);
        if (bodyLen < 4 || bodyLen > 1024 * 1024)
        {
            length = 0;
            return false;
        }

        int total = 4 + bodyLen;
        if (length < total) return false;      // 只收到一半，等下次

        msgId = BitConverter.ToInt32(buffer, 4);
        body = new byte[bodyLen - 4];
        Array.Copy(buffer, 8, body, 0, body.Length);

        Array.Copy(buffer, total, buffer, 0, length - total);
        length -= total;

        return true;
    }
}

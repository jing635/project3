using System;
using System.Collections.Generic;
using System.Text;

// 往 byte[] 里写数据
public class ByteWriter
{
    private readonly List<byte> list = new List<byte>();

    public void WriteInt(int v)
    {
        list.AddRange(BitConverter.GetBytes(v));
    }

    public void WriteString(string s)
    {
        if (s == null) s = "";
        byte[] b = Encoding.UTF8.GetBytes(s);
        WriteInt(b.Length);         
        list.AddRange(b);
    }

    public byte[] ToArray()
    {
        return list.ToArray();
    }
}

// 从 byte[] 里读数据
public class ByteReader
{
    private readonly byte[] data;
    private int pos;

    public ByteReader(byte[] data)
    {
        this.data = data;
        pos = 0;
    }

    public int ReadInt()
    {
        int v = BitConverter.ToInt32(data, pos);
        pos += 4;
        return v;
    }

    public string ReadString()
    {
        int len = ReadInt();
        string s = Encoding.UTF8.GetString(data, pos, len);
        pos += len;
        return s;
    }
}
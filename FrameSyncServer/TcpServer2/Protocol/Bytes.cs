using System;
using System.Collections.Generic;
using System.Text;
using TcpServer2;

// 往 byte[] 里写数据
public class ByteWriter
{
    private readonly List<byte> list = new List<byte>();

    public void WriteInt(int v)
    {
        list.AddRange(BitConverter.GetBytes(v));
    }

    public void WriteFloat(float v)
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

    public void WriteCharacters(List<CharacterData> characters)
    {
        WriteInt(characters.Count);
        for (int i = 0; i < characters.Count; i++)
        {
            WriteCharacter(characters[i]);
        }
    }

    private void WriteCharacter(CharacterData data)
    {
        WriteInt(data.CharacterId);
        WriteString(data.CharacterName);
        WriteString(data.CharacterImage);
        WriteString(data.Skill1);
        WriteString(data.Skill2);
        WriteString(data.Skill3);
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
    public float ReadFloat()
    {
        float v = BitConverter.ToSingle(data, pos);
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
    public List<CharacterData> ReadCharacters()
    {
        int count = ReadInt();
        List<CharacterData> characterdatas = new List<CharacterData>();
        for (int i = 0; i < count; i++)
        { 
            characterdatas.Add(ReadCharacter());
        }
        return characterdatas;

    }
    private CharacterData ReadCharacter()
    {
        CharacterData data = new CharacterData();
        data.CharacterId = ReadInt();
        data.CharacterName = ReadString();
        data.CharacterImage = ReadString();
        data.Skill1 = ReadString();
        data.Skill2 = ReadString();
        data.Skill3 = ReadString();
        return data;
    }
}
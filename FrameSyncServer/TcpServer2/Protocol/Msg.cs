// ============ 消息号 ============
using TcpServer2;

public static class MsgId
{
    public const int LoginReq = 101;   // 客户端 → 服务端：登录
    public const int LoginRes = 102;   // 服务端 → 客户端：登录结果
    public const int RegisterReq = 103;   // 客户端 → 服务端：注册
    public const int RegisterRes = 104;   // 服务端 → 客户端：注册结果
    public const int KickNotify = 105;   // 服务端 → 客户端：你被顶号了
    public const int SaveSettingReq = 201;   // 客户端 → 服务端：保存设置
    public const int SaveSettingRes = 202;   // 服务端 → 客户端：保存结果

    
}

// ============ 结果码 ============
public static class Code
{
    public const int Ok = 0;   // 成功
    public const int UserNotFound = 1;   // 用户不存在
    public const int WrongPassword = 2;   // 密码错误
    public const int UserExists = 3;   // 用户名已被占用
}

// ============ 消息基类 ============
public abstract class BaseMsg
{
    public abstract int Id();
    public abstract byte[] Write();
    public abstract void Read(byte[] body);
}

// ============ 登录 ============
public class LoginReq : BaseMsg
{
    public string username = "";
    public string password = "";

    public override int Id() { return MsgId.LoginReq; }

    public override byte[] Write()
    {
        ByteWriter w = new ByteWriter();
        w.WriteString(username);
        w.WriteString(password);
        return w.ToArray();
    }

    public override void Read(byte[] body)
    {
        ByteReader r = new ByteReader(body);
        username = r.ReadString();
        password = r.ReadString();
    }
}

public class LoginRes : BaseMsg
{
    public int code;
    public int userId;
    public string message = "";
    public bool isPlayMusic;
    public bool isPlaySound;
    public float music;        
    public float sound;
    public List<CharacterData> characterdatas = new List<CharacterData>();

    public override int Id() { return MsgId.LoginRes; }

    public override byte[] Write()
    {
        ByteWriter w = new ByteWriter();
        w.WriteInt(code);
        w.WriteInt(userId);
        w.WriteString(message);
        w.WriteInt(isPlayMusic ? 1 : 0);
        w.WriteInt(isPlaySound ? 1 : 0);
        w.WriteFloat(music);
        w.WriteFloat(sound);
        w.WriteCharacters(characterdatas);
        return w.ToArray();
    }

    public override void Read(byte[] body)
    {
        ByteReader r = new ByteReader(body);
        code = r.ReadInt();
        userId = r.ReadInt();
        message = r.ReadString();
        isPlayMusic = r.ReadInt() == 1;
        isPlaySound = r.ReadInt() == 1;
        music = r.ReadFloat();
        sound = r.ReadFloat();
        characterdatas = r.ReadCharacters();
    }
}

// ============ 注册 ============
public class RegisterReq : BaseMsg
{
    public string username = "";
    public string password = "";

    public override int Id() { return MsgId.RegisterReq; }

    public override byte[] Write()
    {
        ByteWriter w = new ByteWriter();
        w.WriteString(username);
        w.WriteString(password);
        return w.ToArray();
    }

    public override void Read(byte[] body)
    {
        ByteReader r = new ByteReader(body);
        username = r.ReadString();
        password = r.ReadString();
    }
}

public class RegisterRes : BaseMsg
{
    public int code;
    public string message = "";

    public override int Id() { return MsgId.RegisterRes; }

    public override byte[] Write()
    {
        ByteWriter w = new ByteWriter();
        w.WriteInt(code);
        w.WriteString(message);
        return w.ToArray();
    }

    public override void Read(byte[] body)
    {
        ByteReader r = new ByteReader(body);
        code = r.ReadInt();
        message = r.ReadString();
    }
}

// ============ 顶号通知 ============
public class KickNotify : BaseMsg
{
    public string reason = "";

    public override int Id() { return MsgId.KickNotify; }

    public override byte[] Write()
    {
        ByteWriter w = new ByteWriter();
        w.WriteString(reason);
        return w.ToArray();
    }

    public override void Read(byte[] body)
    {
        ByteReader r = new ByteReader(body);
        reason = r.ReadString();
    }
}

// ============ 保存设置 ============
public class SaveSettingReq : BaseMsg
{
    public bool isPlayMusic;
    public bool isPlaySound;
    public float music;
    public float sound;

    public override int Id() { return MsgId.SaveSettingReq; }

    public override byte[] Write()
    {
        ByteWriter w = new ByteWriter();
        w.WriteInt(isPlayMusic ? 1 : 0);
        w.WriteInt(isPlaySound ? 1 : 0);
        w.WriteFloat(music);
        w.WriteFloat(sound);
        return w.ToArray();
    }

    public override void Read(byte[] body)
    {
        ByteReader r = new ByteReader(body);
        isPlayMusic = r.ReadInt() != 0;
        isPlaySound = r.ReadInt() != 0;
        music = r.ReadFloat();
        sound = r.ReadFloat();
    }
}

public class SaveSettingRes : BaseMsg
{
    public int code;

    public override int Id() { return MsgId.SaveSettingRes; }

    public override byte[] Write()
    {
        ByteWriter w = new ByteWriter();
        w.WriteInt(code);
        return w.ToArray();
    }

    public override void Read(byte[] body)
    {
        code = new ByteReader(body).ReadInt();
    }
}

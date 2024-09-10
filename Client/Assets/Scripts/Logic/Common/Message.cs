using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessageManager
{

    public class Message
    {
        public Message(string name)
        {
            this.name = name;
        }
        public string name;
        public MessageDelegate messageCall;
        public object[] ps;
    }

    public delegate void MessageDelegate(Message m);

    private Dictionary<string, List<Message>> _mesageMap = new Dictionary<string, List<Message>>();

    private static MessageManager _instance;
    public static MessageManager Instance
    {
        get {
                if (_instance == null)
                {
                    _instance = new MessageManager();
                }
                return _instance;
            }
    }

    public void SendMessage(string msgName , params object[] ps)
    {
        Message m = BeginMessage(msgName);
        m.ps = ps;
        SendMessage(m);
    }

    public void SendMessage(Message msg)
    {
        DispatchMessage(msg);
    }

    public Message RegisterMessage(string msgName , MessageDelegate messageCall)
    {
        Message m = BeginMessage(msgName);
        m.messageCall = messageCall;

        if (!_mesageMap.ContainsKey(m.name))
        {
            _mesageMap[m.name] = new List<Message>();
        }

        if (!_mesageMap[m.name].Contains(m))
        {
            _mesageMap[m.name].Add(m);
        }
        return m;
    }

    public void RemoveMessage(string msgName, MessageDelegate messageCall)
    {
        if (!_mesageMap.ContainsKey(msgName)) return;

        int inIndex = -1;
        for(int i = 0; i < _mesageMap[msgName].Count; i++)
        {
            if (_mesageMap[msgName][i].messageCall == messageCall)
            {
                inIndex = i;
                break;
            }
        }
        if(inIndex != -1)
        {
            _mesageMap[msgName].RemoveAt(inIndex);
        }
    }

    public void RegisterMessage(Message m)
    {
        if (!_mesageMap.ContainsKey(m.name))
        {
            _mesageMap[m.name] = new List<Message>();
        }

        if (!_mesageMap[m.name].Contains(m))
        {
            _mesageMap[m.name].Add(m);
        }
    }

    public void RemoveMessage(Message m)
    {
        if (!_mesageMap.ContainsKey(m.name)) return;

        if (!_mesageMap[m.name].Contains(m)) return;

        _mesageMap[m.name].Remove(m);
    }

    public Message BeginMessage(string msgName)
    {
        Message data = new Message(msgName);
        return data;
    }
    

    private void DispatchMessage(Message msg)
    {
        if (!_mesageMap.ContainsKey(msg.name)) return;

        for(int i = 0; i< _mesageMap[msg.name].Count; i++)
        {
            Message m = _mesageMap[msg.name][i];
            m.messageCall.Invoke(msg);
        }
    }
}

public static class MessageConst
{
    public static string S2C_Tut_End = "1";

    // UI相关
    public static string UI_Open = "1001";
    public static string UI_Close = "1002";
    public static string UI_Click = "1003";
    public static string Tut_End = "2001";

    public static string Battle_UI_SelectTower = "Battle_UI_SelectTower";
    public static string Battle_UI_SelectCusion = "Battle_UI_SelectCusion";
    public static string Battle_UI_TowerPlayDownOrUp = "Battle_UI_TowerPlayDownOrUp";
    public static string Battle_UI_CusionPlayDownOrUp = "Battle_UI_CusionPlayDownOrUp";

    // 大世界
    public static string Battle_BigWorld_Create = "Battle_BigWorld_Create";

    // 战斗相关
    public static string Battle_BattleStart = "Battle_BattleStart";
    public static string Battle_BattleFail = "Battle_BattleFail";
    public static string Battle_BattleExit = "Battle_BattleExit";
    public static string Battle_BattleSuccess = "Battle_BattleSuccess";

    public static string Battle_EnemyEnter = "Battle_EnemyEnter";   // 敌人进入战斗
    public static string Battle_EnemyDie = "Battle_EnemyDie";       // 敌人死亡
    public static string Battle_EnemyExit = "Battle_EnemyExit";     // 敌人到达终点

    public static string Battle_Collision = "Battle_Collision";

    public static string Battle_EntityHurt = "Battle_EntityHurt";

}
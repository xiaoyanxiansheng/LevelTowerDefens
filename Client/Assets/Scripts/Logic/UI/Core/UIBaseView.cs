/*
UI分类
    1 主UI(自动流程)：每个UI集合必须有一个，打开UI集合前需要关闭当前UI集合，这时只需要处理打开逻辑就行；关闭当前UI集合后需要打开上次关闭的UI集合，这时只需要处理关闭逻辑就行。
    2 主UI(手动流程)：每个UI集合必须有一个，不受自动打开和关闭的影响。比如一些弹出框。
    0 子UI：和主UI一起构成UI集合
*/

using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static MessageManager;

public abstract class UIBaseView
{
    public string name = "";
    private List<object> openParams = new List<object>();
    private int uiInitRequestId = 0;		// 异步请求ID
    private Action uiInitFinishCall;	// 请求加载完成后的回调
    private int uiInstanceId = 0;			// UI的实例ID
    private GameObject uiBindCore;			// 绑定Prefab中节点控件
    private List<MessageManager.Message> registerMessages = new List<MessageManager.Message>();	// 注册的消息

    // 打开关闭流程
    private bool isShow = false;			// 当前UI是否显示

    protected UIBaseView(string name)
    {
        this.name = name;
        if (openParams != null) openParams.Clear();
    }

    public void SetOpenParams(List<object> openParams)
    {
        this .openParams = openParams;
    }

    public void Init(Action initFinishCall)
    {
        // 加载中
        if (IsIniting())
        {
            return;
        }

        // 加载完
        if (IsInit())
        {
            if (initFinishCall != null)
            {
                initFinishCall();
            }
            return;
        }

        // 未加载
        uiInitFinishCall = initFinishCall;
        ShowFullScreenMask();
        uiInitRequestId = ResourceManager.CreateGameObjectAsync(LoadGameObjectType.UI , name, (instanceId , requestId) => { OnCreateInstance(instanceId , requestId); });
    }

    private void OnCreateInstance(int instanceId , int requestId)
    {
        if(uiInitRequestId != requestId)
        {
            Debug.LogError("UIBaseView 加载错误");
            return;
        }

        CloseFullScreenMask();

        if(instanceId == 0)
        {
            Debug.LogError("OnCreateInstance is error " + name);
            return;
        }

        uiInitRequestId = 0;
        uiInstanceId = instanceId;

        GameObject ui = GetGameObject();
        var trams = ui.GetComponent<RectTransform>();
        trams.sizeDelta = Vector2.zero;

        // 加载完成
        OnCreate();

        // 绑定UICore 应该不需要了
        BindUICore();

        //  注册UI事件
        OnRegisterMessage();

        if(uiInitFinishCall != null)
        {
            uiInitFinishCall();
            uiInitFinishCall = null;
        }
    }

    public void Show(bool isBack)
    {
        if (IsShow())
        {
            OnShow();
            return;
        }

        // 计算层级
        SetUILayer();
        // 加载图集
        LoadAtlas(() => { OnShowBefore(isBack); });
    }

    public void Close(bool isDestory, Action closeFinishCall)
    {
        if (!IsShow())
        {
            CloseAfter(isDestory,closeFinishCall);
            return;
        }

        CloseFullScreenMask();

        // 1 释放图集
        ReleaseAtlas();

        // 2 关闭GameObject
        GameObject obj = GetGameObjectById(uiInstanceId);
        if(obj == null)
        {
            Debug.LogError("Close is Error" + name);
            return;
        }

        isShow = false;
        obj.SetActive(false);

        // 3 删除UI层级
        DelUILayer();

        // 4 关闭完成
        OnClose();

        // 5 关闭之后的流程
        CloseAfter(isDestory, closeFinishCall);
    }

    private void CloseAfter(bool isDestory, Action closeFinishCall)
    {
        if(closeFinishCall != null)
        {
            closeFinishCall();
        }

        if(isDestory)
        {
            UnInit();
        }
    }

    private void UnInit()
    {
        if (IsShow())
        {
            Debug.LogError("Please Close it first " + name);
            return;
        }

        // 取消加载
        if (IsIniting())
        {
            ResourceManager.CancelCreateAssetAsync(uiInitRequestId);
            uiInitRequestId = 0;
            return;
        }

        // 已经卸载
        if (!IsInit())
        {
            return;
        }

        // 1 清理消息
        RemoveRegisterMesssge();
        // 2 解绑UICore
        UnBindUICore();
        // 3 卸载GameObject
        DestoryGameObject(uiInstanceId);
        // 4 数据清理
        ClearParams();
        // 5 卸载完成
        OnDestory();
        uiInstanceId = 0;
    }

    private void BindUICore()
    {
        // TODO 3 需要处理
    }

    private void UnBindUICore()
    {
        // TODO 3 需要处理
    }

    protected List<object> GetParams()
    {
        return openParams;
    }

    private void ClearParams()
    {
        if (openParams != null) openParams.Clear();
    }

    private void OnShowBefore()
    {
        // 获取实例
        GameObject obj = GetGameObjectById(uiInstanceId);
        if(obj == null)
        {
            Debug.LogError("OnShowBefore is Error " + name);
            return;
        }

        // 显示
        isShow = true;
        obj.gameObject.SetActive(isShow);

        // 显示完成
        OnShow();

        // 通知UI已经打开

    }

    public bool IsIniting()
    {
        return uiInitRequestId > 0;
    }

    public bool IsInit()
    {
        return uiInstanceId != 0;
    }

    // TODO 全屏遮罩处理
    public void ShowFullScreenMask() { }
    public void CloseFullScreenMask(){ }


    public bool IsShow()
    {
        return isShow;
    }

    protected void RegisterMessage(string msgName , MessageDelegate messageCall)
    {
        for(int i = 0; i < registerMessages.Count; i++)
        {
            MessageManager.Message m = registerMessages[i];
            if(m.name == msgName && m.messageCall == messageCall)
            {
                return;
            }
        }

        MessageManager.Message nm = MessageManager.Instance.BeginMessage(msgName);
        nm.messageCall = messageCall;
        MessageManager.Instance.RegisterMessage(nm);
        registerMessages.Add(nm);
    }

    private void RemoveRegisterMesssge() 
    { 
        for(int i = 0; i < registerMessages.Count; i++)
        {
            MessageManager.Instance.RemoveMessage(registerMessages[i]);
        }
        registerMessages.Clear();
    }

    private void SetUILayer()
    {
        // TODO 0 需要动态计算层级
        GetGameObject().transform.SetSiblingIndex(UIManager.Instance.UIRoot.transform.childCount);
    }

    private void DelUILayer()
    {
        // TODO 0 需要动态计算层级
    }

    private void LoadAtlas(Action finishCall)
    {
        // TODO 需要处理
        finishCall();
    }

    private void ReleaseAtlas()
    {
        // TODO 需要处理
    }

    private void OnShowBefore(bool isBack)
    {
        GameObject obj = GetGameObjectById(uiInstanceId);
        if(obj == null )
        {
            Debug.LogError("OnShowBefore is Error " + name);
            return;
        }

        // 显示
        isShow = true;
        obj.SetActive(isShow);

        // 显示完成
        OnShow();

        // 通知UI已经打开
        MessageManager.Instance.SendMessage(MessageManager.Instance.BeginMessage(MessageConst.UI_Open));
    }

    public bool IsAutoMainUI()
    {
        UIType uIType = UIConfig.GetUIType(name);
        return uIType == UIType.AutoMain;
    }

    public bool IsMainUI()
    {
        UIType uIType = UIConfig.GetUIType(name);
        return uIType == UIType.Main;
    }

    public bool IsSonUI()
    {
        UIType uIType = UIConfig.GetUIType(name);
        return uIType == UIType.Son;
    }

    private GameObject GetGameObjectById(int uiInstanceId)
    {
        return ResourceManager.GetGameObjectById(uiInstanceId);
    }

    private void DestoryGameObject(int uiInstanceId)
    {
        ResourceManager.DestoryGameObject(uiInstanceId);
    }

    protected void RegisterButtonClick(string path ,UnityAction clickCall)
    {
        GameObject o = GetGameObject(path);
        if(o == null)
        {
            Debug.LogError("RegisterButtonClick is Error " + path + " " + name);
            return;
        }
        o.GetComponent<Button>().onClick.AddListener(clickCall);
    }

    protected void RegisterButtonClick(string path, Action<int> clickCall , int p)
    {
        GameObject o = GetGameObject(path);
        if (o == null)
        {
            Debug.LogError("RegisterButtonClick is Error " + path + " " + name);
            return;
        }
        o.GetComponent<Button>().onClick.AddListener(() => {
            clickCall(p);
        });
    }

    protected void RegisterButtonClick(Button btn, Action<int> clickCall, int p)
    {
        btn.onClick.AddListener(() => {
            clickCall(p);
        });
    }

    protected void RegisterButtonClick(Button btn, UnityAction clickCall)
    {
        btn.onClick.AddListener(clickCall);
    }

    protected GameObject GetGameObject()
    {
        return GetGameObjectById(uiInstanceId).gameObject;
    }

    protected GameObject GetGameObject(string path)
    {
        return GetGameObjectById(uiInstanceId).transform.Find(path).gameObject;
    }

    protected int GetInstanceId()
    {
        return uiInstanceId;
    }

    #region 子类重写
    // 加载完成
    public abstract void OnCreate();
    // 注册事件
    public abstract void OnRegisterMessage();
    // 打开完成
    public abstract void OnShow(bool isBack = false);
    // 关闭完成
    public abstract void OnClose();

    // 卸载完成
    public abstract void OnDestory();
    #endregion

    protected void OnBack()
    {
        UIManager.Instance.Close(name,true);
    }
}

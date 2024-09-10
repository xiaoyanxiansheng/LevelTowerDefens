
using System;
using System.Collections.Generic;
using System.Linq;

public class UIBaseCollect
{

    private List<object> _params;                                       // 打开页面参数
    private List<UIBaseView> initViews = new List<UIBaseView>();        // 已经加载
    private List<UIBaseView> openingViews = new List<UIBaseView>();     // 加载但是未打开
    private List<UIBaseView> openedViews = new List<UIBaseView>();      // 已经打开

    public void Init(List<object> ps, Action initFinishCall , List<UIBaseView> uiViews)
    {

        if (!CheckViews(uiViews)) return;

        // UI页面参数
        this._params = ps;
        // 加载列表
        AddInitView(uiViews);

        // 加载前开启全屏屏蔽UI：防止看到场景和防止误触
        ShowFullScreenMask();

        // 加载所有UI 全部加载完成后才算加载完成
        int curInitCount = 0;
        int totalInitCount = uiViews.Count;
        for(int i = 0; i < totalInitCount; i++)
        {
            uiViews[i].SetOpenParams(ps);
            uiViews[i].Init(() =>
            {
                curInitCount++;
                // 加载完成
                if(curInitCount == totalInitCount)
                {
                    // 关闭全屏Mask
                    CloseFullScreenMask();
                    AddOpeningViews(uiViews);
                    if (initFinishCall != null)
                    {
                        initFinishCall();
                    }
                }
            });
        }
    }

    public void Show(bool isBack = false)
    {
        if (!CheckViews(openingViews)) return;

        // 打开所有等待打开的UI
        for(int i = 0;i < openingViews.Count; i++)
        {
            openingViews[i].Show(isBack);
        }

        // 改变状态
        AddOpenedViews(openingViews);
        openingViews.Clear();
    }

    public void CloseAll(Action closeFinishCall , bool isDestory = false)
    {
        // 卸载流程处理所有UI，关闭流程只需要处理已经打开的
        List<UIBaseView> uiViews = isDestory ? initViews : openedViews;
        if (initViews.Count == 0) return;

        // 从后往前关闭UI
        int uiViewCout = uiViews.Count;
        for(int i = uiViewCout-1; i >= 0 ; i--)
        {
            UIBaseView uiView = uiViews[i];
            // 这里需要注意 只有自动流程主UI才有权利完成之后返回
            Close(uiView, uiView.IsAutoMainUI() ? closeFinishCall : null, isDestory);
        }
    }

    // 关闭UI集合并且保存当前打开UI到待打开状态（目的：去往某个界面之后可以重新打开之前的UI集合，eg：战斗结束返回之前的界面）
    public void CloseAllAndSave(Action closeFinishCall , bool isDestory = false)
    {
        // 保存之前的打开状态
        List<UIBaseView> uiViews = new List<UIBaseView> ();
        for(int i = 0; i < openedViews.Count; i++)
        {
            uiViews.Add(openedViews[i]);
        }

        // 关闭当前打开的UI集合
        CloseAll(closeFinishCall , isDestory);

        // 加入到保存列表中 等待返回流程的时候打开
        AddOpeningViews(uiViews);
    }

    // 关闭UI
    public void Close(UIBaseView uiView , Action closeFinishCall = null , bool isDestory = false)
    {
        RemoveOpeningView(uiView);
        RemoveOpendedView(uiView);
        if(isDestory)
        {
            RemoveInitView(uiView);
        }
        uiView.Close(isDestory, closeFinishCall);
    }

    private void AddInitView(List<UIBaseView> uiViews , bool isReset = false)
    {
        if (isReset) initViews.Clear();

        initViews.AddRange(uiViews);
    }

    private void AddOpeningViews(List<UIBaseView> uiViews , bool isReset = false)
    {
        if (isReset)
        {
            openingViews.Clear();
        }

        openingViews.AddRange(uiViews);
    }

    private void AddOpenedViews(List<UIBaseView> uiViews , bool isReset = false)
    {
        if(isReset) 
        {
            openedViews.Clear();
        }

        openedViews.AddRange(uiViews);
    }

    private void RemoveInitView(UIBaseView uiView)
    {
        initViews.Remove(uiView);
    }
    private void RemoveOpeningView(UIBaseView uiView)
    {
        openingViews.Remove(uiView);
    }
    private void RemoveOpendedView(UIBaseView uiView)
    {
        openedViews.Remove(uiView);
    }

    public List<UIBaseView> GetInitViews()
    {
        return initViews;
    }

    // 将要打开的UI是否包含主UI
    public bool IsContainAutoMainUI()
    {
        foreach (UIBaseView view in openingViews)
        {
            if (view.IsAutoMainUI())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAllSonUI()
    {
        foreach (UIBaseView view in openingViews)
        {
            if (view.IsSonUI())
            {
                return false;
            }
        }
        return true;
    }

    public bool IsAotuMainCollet()
    {
        return GetMain().IsAutoMainUI();
    }

    public bool IsMainCollet()
    {
        return GetMain().IsMainUI();
    }

    private bool CheckViews(List<UIBaseView> uiViews)
    {
        return uiViews != null && uiViews.Count > 0;
    }

    private void ShowFullScreenMask()
    {
        // TODO 5
    }

    private void CloseFullScreenMask()
    {
        // TODO 5
    }

    public UIBaseView GetMain()
    {
        return initViews[0];
    }

    public bool IsAutoMainCollect()
    {
        return GetMain().IsAutoMainUI();
    }
}

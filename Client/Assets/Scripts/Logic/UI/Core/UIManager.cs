/*
    UI对外部提供的管理类

    加载 打开 关闭 卸载
--*/

using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager{
    public static UIManager Instance;

    public GameObject UIRoot;

    public Canvas canvas;

    private List<UIBaseView> _uiViews = new List<UIBaseView>();
    private List<UIBaseCollect> _initViewCollects = new List<UIBaseCollect>();
    private List<UIBaseCollect> _openViewCollects = new List<UIBaseCollect>();

    public static void Create()
    {
        if (Instance != null) return;

        new UIManager();
    }

    public UIManager()
    {
        Instance = this;
        canvas = GameObject.Find("UIRoot").GetComponent<Canvas>();
        UIRoot = canvas.transform.Find("UI/Bottom").gameObject;
    }

    public void InitBase()
    {
        ResourceManager.CreateGameObjectAsync(LoadGameObjectType.UI, "ui/uiprefab/ui_base", (int instanceId, int requestId) =>
        {
            GameObject obj = ResourceManager.GetGameObjectById(instanceId);
            if(obj != null)
            {
                TrimGameObejct(obj);
                obj.SetActive(true);
                // TODO 0 加载场景时不要删除
                OpenMainUI();
            }
            else
            {
                Debug.LogError("UIManager init is error ");
            }
        });
    }

    private void OpenMainUI()
    {
        // Open(UIViewName.UIBigWorld);
    }

    private void TrimGameObejct(GameObject go)
    {
        if (go == null) return;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(1, 1, 1);
        go.transform.localRotation = Quaternion.identity;
    }

    #region 对外提供
    // 加载UI
    public void Init(List<object> ps , Action<UIBaseCollect> initFinishCall , params string[] viewNames)
    {
        List<UIBaseView> views = new List<UIBaseView>();
        for(int i = 0; i < viewNames.Length; i++)
        {
            views.Add(UIConfig.GetUIView(viewNames[i]));
        }
        // 获取集合
        UIBaseCollect viewCollect = GetViewCollect(views[0]);
        if(viewCollect == null)
        {
            Debug.LogError("UIManager init is error");
            return;
        }

        _initViewCollects.Remove(viewCollect);
        _initViewCollects.Add(viewCollect);

        viewCollect.Init(ps, () =>
        {
            if (initFinishCall != null)
            {
                initFinishCall(viewCollect);
            }
        }, views);
    }

    // 打开UI或者UI集合 
    // 1 当前UI含主UI 关闭上一个UI集合再打开当前
    // 2 当前UI不含主UI 直接打开UI
    public void Open(object p, params string[] uiNames)
    {
        Open(new List<object>() { p }, uiNames);
    }
    public void Open(object p1, object p2, params string[] uiNames)
    {
        Open(new List<object>() { p1,p2 }, uiNames);
    }
    public void Open(List<object> ps, params string[] uiNames)
    {
        Init(ps, (UIBaseCollect viewCollect) =>
        {
            UIBaseCollect topViewCollect = GetTopAutoShowMainViewCollect();
            if(topViewCollect != null && topViewCollect != viewCollect)
            {
                // 只有主UI并且类型为1的才会进入自动流程
                if (viewCollect.IsAotuMainCollet())
                {
                    // 先关闭当前UI集合 然后在打开下一个UI集合
                    CloseViewCollect(topViewCollect, () =>
                    {
                        OpenViewCollect(viewCollect);
                    }, false , true);
                }
                else
                {
                    OpenViewCollect(viewCollect);
                }
            }
            else
            {
                OpenViewCollect(viewCollect);
            }
        }, uiNames);
    }

    public void Open(params string[] uiNames)
    {
        Open(null, uiNames);
    }

    public void Close(string viewName , bool isDestory = false)
    {
        UIBaseView view = UIConfig.GetUIView(viewName);
        UIBaseCollect viewCollect = GetViewCollect(view);
        if(viewCollect == null) 
        {
            Debug.LogError("UIManager Close Error");
            return;
        }

        if(view.IsAutoMainUI())
        {
            // 先关闭UI集合 然后在打开上次关闭的UI集合
            _openViewCollects.Remove(viewCollect);
            CloseViewCollect(viewCollect, () =>
            {
                UIBaseCollect topViewCollect = GetTopAutoMainViewCollect();
                if(topViewCollect != null)
                {
                    OpenViewCollect(topViewCollect,true);
                }
            }, isDestory);
        }
        else
        {
            viewCollect.Close(view);
        }
    }

    public void OpenViewCollect(UIBaseCollect viewCollect , bool isBack = false)
    {
        _openViewCollects.Remove(viewCollect);
        _openViewCollects.Add(viewCollect);
        viewCollect.Show(isBack);
    }

    public void CloseViewCollect(UIBaseCollect viewCollect , Action closeFinishCall , bool isDestory , bool isBack = false)
    {
        if(isDestory)
        {
            _initViewCollects.Remove(viewCollect);
        }

        if(isBack)
        {
            viewCollect.CloseAllAndSave(closeFinishCall, isDestory);
        }
        else
        {
            viewCollect.CloseAll(closeFinishCall, isDestory);
        }
    }

    #endregion

    #region 不对外提供
    // 获取显示UI集合(最上层自动流程集合：就是主UI类型为1的UI集合)
    // 因为主UI分两种：类型1 会被自动打开关闭影响，类型2 不会；这里需要的是类型1的主UI
    private UIBaseCollect GetTopAutoShowMainViewCollect()
    {
        for (int i = _openViewCollects.Count - 1; i >= 0; i--)
        {
            if (_openViewCollects[i].IsAutoMainCollect()) return _openViewCollects[i];
        }
        return null;
    }

    // 获取UI集合
    private UIBaseCollect GetTopAutoMainViewCollect()
    {
        for (int i = _openViewCollects.Count - 1; i >= 0; i--)
        {
            if(_openViewCollects[i].IsAutoMainCollect())return _initViewCollects[i];
        }
        return null;
    }

    // 获取UI集合
    private UIBaseCollect GetViewCollect(UIBaseView uiView)
    {
        for(int i = _initViewCollects.Count - 1; i >= 0; i--)
        {
            foreach(UIBaseView view in _initViewCollects[i].GetInitViews())
            {
                if (uiView == view) return _initViewCollects[i];
            }
        }
        return new UIBaseCollect();
    }

    #endregion
}

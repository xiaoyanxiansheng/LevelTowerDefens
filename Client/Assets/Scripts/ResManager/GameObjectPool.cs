using UnityEngine;
using System.Collections.Generic;

public class GameObjectPool {

    public delegate void OnCreateGameObject(int instanceId,int requestId);

    public class GameObjectRef
    {
        public void Clear()
        {
            instanceId = 0;
            assetName = null;
            obj = null;
        }
        public int instanceId;
        public string assetName;
        public GameObject obj;
    }

    private static Dictionary<int, GameObjectRef> _gameObjectRef = new Dictionary<int, GameObjectRef>();
    private static List<GameObjectRef> _freeGameObjectRef = new List<GameObjectRef>();

    private static GameObjectRef GetTempGameObjectRef()
    {
        GameObjectRef gameObjectRef = null;
        if (_freeGameObjectRef.Count > 0)
        {
            gameObjectRef = _freeGameObjectRef[0];
            _freeGameObjectRef.RemoveAt(0);
        }
        else
        {
            gameObjectRef = new GameObjectRef();
        }
        return gameObjectRef;
    }
    private static void RecoveryGameObjectRef(GameObjectRef gameObjectRef)
    {
        if (gameObjectRef == null) return;
        if (gameObjectRef.obj != null)
        {
            GameObject.Destroy(gameObjectRef.obj);
            ReferenceObject.ReleaseObject(gameObjectRef.assetName);
        }
        if (ResourceManager.isLog) Debug.Log("gameObejct Destory " + gameObjectRef.assetName);
        if (_gameObjectRef.ContainsKey(gameObjectRef.instanceId))
            _gameObjectRef.Remove(gameObjectRef.instanceId);
        gameObjectRef.Clear();
        _freeGameObjectRef.Add(gameObjectRef);
    }

    public static int AddGameObject(LoadGameObjectType type,string assetName,Object asset)
    {
        if (asset == null) return 0;
        if (!(asset is GameObject))
        {
            Debug.LogError(assetName + " is not a GameObject");
            return 0;
        }

        if (ResourceManager.isLog) Debug.Log("gameObject create " + assetName);

        GameObject obj = (GameObject)GameObject.Instantiate(asset,Vector3.zero,Quaternion.identity);
        obj = InitGameObject(type, obj, assetName);
        obj.SetActive(false);
        int instanceId = obj.GetInstanceID();

        GameObjectRef gameObjectRef = GetTempGameObjectRef();
        gameObjectRef.assetName = assetName;
        gameObjectRef.instanceId = instanceId;
        gameObjectRef.obj = obj;
        _gameObjectRef.Add(instanceId, gameObjectRef);

        return instanceId;
    }

    public static GameObject InitGameObject(LoadGameObjectType type, GameObject obj, string assetName)
    {
        Transform parent = null;
        Transform son = null;
        // GameObject
        if (type == LoadGameObjectType.GameObject)
        {
            son = obj.transform;
        }
        // UI
        else if(type == LoadGameObjectType.UI)
        {
            GameObject root = GameObject.Find("UIRoot/UI/Bottom");
            parent = root.transform;
            //son = obj.transform.Find("Core");
            //GameObject.Destroy(obj);
            son = obj.transform;
            son.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }

        if (son != null)
        {
            son.name = assetName;
            if (parent != null)
            {
                son.SetParent(parent);
            }
            TrimGameObejct(son.gameObject);
        }

        return son.gameObject;
    }

    private static void TrimGameObejct(GameObject go)
    {
        if (go == null) return;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(1, 1, 1);
        go.transform.localRotation = Quaternion.identity;
    }

    public static void DestoryGameObject(int instanceId)
    {
        if (!_gameObjectRef.ContainsKey(instanceId)) return;

        RecoveryGameObjectRef(_gameObjectRef[instanceId]);
    }
    public static GameObject GetGameObject(int instanceId)
    {
        if (!_gameObjectRef.ContainsKey(instanceId)) return null;

        return _gameObjectRef[instanceId].obj;
    }
}

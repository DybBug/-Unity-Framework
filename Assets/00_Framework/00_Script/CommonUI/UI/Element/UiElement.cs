using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BindingData
{
    [SerializeField] public string Key;
    [SerializeField] public RectTransform BindingObject;
}

[System.Serializable]
public abstract class UiElement : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private List<BindingData> m_BindingObjects = new();

    [HideInInspector]
    private Dictionary<string, RectTransform> m_CachedObjectMap = new();
    
    public bool IsEnable { get; private set; }

    #region Unity
    private void Awake()
    {
        for(int i = 0; i < m_BindingObjects.Count; ++i)
        {
            string key = m_BindingObjects[i].Key;
            RectTransform bindingObj = m_BindingObjects[i].BindingObject;
            m_CachedObjectMap.Add(key, bindingObj);
        }
        m_BindingObjects.Clear();
    }

    private void Start()
    {
        //for(var iter = m_CachedObjectMap.GetEnumerator(); iter.MoveNext(); )
        //{
        //    string key = iter.Current.Key;
        //    var gameObj = iter.Current.Value;
        //    Debug.Log($"{key} : {gameObj.name}");
        //}
    }

    private void Update()
    {
        if (!IsEnable)
            return;

        Tick(Time.deltaTime);
    }
    #endregion

    #region FindObject
    public GameObject FindObjectOrNull(string _key)
    {
        RectTransform value;
        if(m_CachedObjectMap.TryGetValue(_key, out value))
        {
            return value.gameObject;
        }
        return null;
    }

    public TComponent FindComponentOrNull<TComponent>(string _key) where TComponent : Component
    {
        return FindObjectOrNull(_key)?.GetComponent<TComponent>() ?? null;
    }

    public TComponent FindComponentOrNull<TComponent>(Enum _enum) where TComponent : Component
    {
        string key = _enum.ToString();
        return FindComponentOrNull<TComponent>(key);
    }
    #endregion

    public void Initialize() => OnInitialize();
    public void Tick(float dt) => OnTick(dt);
    public void Enable() 
    {
        IsEnable = true;
        gameObject.SetActive(true);
    }
    public void Disable()
    {
        IsEnable = false;
        gameObject.SetActive(false);
    }

    protected abstract void OnInitialize();
    protected abstract void OnTick(float dt);
}


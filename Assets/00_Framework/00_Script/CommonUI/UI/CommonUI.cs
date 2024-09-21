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
public class CommonUI :MonoBehaviour
{

    [SerializeField, HideInInspector]
    private List<BindingData> m_BindingObjects = new();

    [HideInInspector]
    private Dictionary<string, RectTransform> m_CachedObjectMap = new();

    private void Awake()
    {
        for(int i = 0; i < m_BindingObjects.Count; ++i)
        {
            string key = m_BindingObjects[i].Key;
            RectTransform bindingObj = m_BindingObjects[i].BindingObject;
            m_CachedObjectMap.Add(key, bindingObj);
        }
    }

    private void Start()
    {
        for(var iter = m_CachedObjectMap.GetEnumerator(); iter.MoveNext(); )
        {
            string key = iter.Current.Key;
            var gameObj = iter.Current.Value;
            Debug.Log($"{key} : {gameObj.name}");
        }
    }

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
}


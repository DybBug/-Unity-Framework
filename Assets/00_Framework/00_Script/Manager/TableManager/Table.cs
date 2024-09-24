using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class TableItem
{
    public abstract string GetKey();
}

public abstract class Table : ScriptableObject
{

}


public class Table<T> : Table where T : TableItem
{
    private static Table<T> _instance;
    public static Table<T> Instance
    {
        get
        {
            Debug.Assert(_instance != null);
            if (_instance.m_ItemsByKey == null)
            {
                _instance.Load();
            }
            return _instance;
        }
    }

    [SerializeField] private List<T> m_Items = new();

    private Dictionary<string/*key*/, T> m_ItemsByKey;

    public Table() : base()
    {
        _instance = this;
    }
    public void Load()
    {
        if (m_ItemsByKey != null)
        {
            m_ItemsByKey.Clear();
            m_ItemsByKey = null;
        }
        m_ItemsByKey = m_Items.ToDictionary(e => e.GetKey(), e => e);
    }


    public IReadOnlyList<string> GetKeys()
    {
        Debug.Assert(m_ItemsByKey != null);
        return m_ItemsByKey.Keys.ToList();
    }

    public T GetItemByKey(string key)
    {
        Debug.Assert(m_ItemsByKey != null);

        var result = m_ItemsByKey.TryGetValue(key, out var item);
        Debug.Assert(result == true, $"{key} is invalid.");

        return item;
    }
}

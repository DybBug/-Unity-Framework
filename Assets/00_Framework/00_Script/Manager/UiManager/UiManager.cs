using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class UiManager : MonoBehaviour
{
    #region Instance
    private static UiManager _instance;
    public static UiManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var gameObject = new GameObject("UiManager");
                _instance = gameObject.AddComponent<UiManager>();
            }
            return _instance;
        }
    }

    #endregion

    private readonly Dictionary<string/*name*/, ViewElement> m_CachedViewElementsByName = new();
    private readonly Dictionary<string/*name*/, ViewElement> m_OpenedViewElementByName = new();

    private Canvas m_Canvas;

    #region Unity
    private void Awake()
    {
        m_Canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
        Debug.Assert(m_Canvas != null);

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    public T MakeElement<T>(string name = null, RectTransform parent = null) where T : UiElement
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }
        var prefab = AssetLoader.LoadPrefab(name);
        var uiElement = Instantiate(prefab, parent).GetComponent<T>();
        uiElement.Initialize();
        return uiElement;
    }

    public T OpenView<T>(string name) where T : ViewElement
    {
        if (!m_CachedViewElementsByName.TryGetValue(name, out var viewElement))
        {
            var prefab = AssetLoader.LoadPrefab(name);
            viewElement = Instantiate(prefab).GetComponent<T>();
            viewElement.Initialize();

            m_CachedViewElementsByName.Add(name, viewElement);
        }

        m_OpenedViewElementByName.Add(name, viewElement);
        viewElement.transform.SetParent(m_Canvas.gameObject.transform, false);
        viewElement.transform.localPosition = Vector3.zero;
        viewElement.Open();
        viewElement.Enable();
        return viewElement as T;
    }

    public void CloseView(string name)
    {
        if (!m_OpenedViewElementByName.TryGetValue(name, out var viewElement))
        {
            return;
        }

        viewElement.transform.SetParent(null);
        viewElement.Close();
        viewElement.Disable();
        m_OpenedViewElementByName.Remove(name);
    }

    public void CloseView(ViewElement viewElement)
    {
        var item = m_OpenedViewElementByName.FirstOrDefault(e => e.Value == viewElement);

        if (item.Equals(default(KeyValuePair)))
            return;

        item.Value.transform.SetParent(null);
        item.Value.Close();
        item.Value.Disable();
        m_OpenedViewElementByName.Remove(item.Key);
    }

}

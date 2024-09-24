using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


public sealed class UiBinderEditor : EditorWindow
{
    private UiElement m_CommonUI;
    private SerializedObject m_SerializedCommonUi;

    private SerializedProperty m_PropBindingObjects;
    private ReorderableList m_reorderableList;

    private readonly Regex m_Regex = new Regex(@"^\${1}");
    private StringBuilder m_StringBuilder = new();

    [MenuItem("CustomEditor/UiBinderEditor")]
    static void OpenUiBinderEditor()
    {
        // 열려있어도 계속 열림
        //var uiBinderWindow = CreateInstance<UI_BinderWindow>();
        //uiBinderWindow.Show();

        // 한번만 열림
        GetWindow<UiBinderEditor>("UI 바인더", true).ShowUtility();
    }


    public void CreateGUI()
    {

    }

    private void OnEnable()
    {
        
    }

    private void OnGUI()
    {      
        EditorGUI.BeginChangeCheck();

        m_CommonUI = EditorGUILayout.ObjectField("바인딩할 UI", m_CommonUI, typeof(UiElement), true) as UiElement;

        // 에디터에 변경이 있는지 체크
        if (EditorGUI.EndChangeCheck())
        {
            if(m_CommonUI != null)
            {
                Setup();
            }
            else
            {
                Clear();
            }
        }

        if (m_CommonUI == null) return;


        if (m_SerializedCommonUi == null) return;

        m_SerializedCommonUi.Update();
        {
            m_PropBindingObjects.ClearArray();
            BindOjects(m_CommonUI.transform, m_PropBindingObjects);

            m_reorderableList.DoLayoutList();

            if (GUILayout.Button("클립보드 복사"))
            {
                CopyToClipboard();
            }

        }
        m_SerializedCommonUi.ApplyModifiedProperties();
    }

    private void Setup()
    {
        m_SerializedCommonUi = new SerializedObject(m_CommonUI);

        m_PropBindingObjects = m_SerializedCommonUi.FindProperty("m_BindingObjects");

        m_reorderableList = new ReorderableList(
            m_SerializedCommonUi,
            m_PropBindingObjects,
            true, true, false, false);

        m_reorderableList.drawElementCallback = DrawReorderableList;
    }

    private void Clear()
    {
        m_SerializedCommonUi = null;
        m_PropBindingObjects = null;
        m_reorderableList = null;
    }

    private void DrawReorderableList(Rect _rect, int _index, bool _active, bool _focused)
    {
        _rect.y += 2;
        var element = m_reorderableList.serializedProperty.GetArrayElementAtIndex(_index);

        var baseWidth = (_rect.width / 3) - 5;

        _rect = new Rect(_rect.x, _rect.y, baseWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.TextField(_rect, element.FindPropertyRelative("Key").stringValue);

        _rect = new Rect(_rect.x + baseWidth + 5, _rect.y, baseWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(_rect, element.FindPropertyRelative("BindingObject"), GUIContent.none);
    }

    private void BindOjects(Transform _transform, SerializedProperty _prop)
    {
        for (int i = 0; i < _transform.childCount; ++i)
        {
            Transform child = _transform.GetChild(i);
            string childName = child.gameObject.name;

            if (m_Regex.IsMatch(childName))
            {
                BindingData data = MakeBindingData(childName, child);
                ApplyBindingObject(_prop, i, data);
 
                if(!child.GetComponent<UiElement>())
                {
                    BindOjects(child, _prop);
                }
            }
        }
    }

    private BindingData MakeBindingData(in string _name, in Transform _gameObject)
    {
        BindingData data;
        data.Key = m_Regex.Replace(_name, string.Empty);
        data.BindingObject = _gameObject as RectTransform;
        return data;
    }

    private void ApplyBindingObject(SerializedProperty _prop,int _index, in BindingData _data)
    {
        _prop.InsertArrayElementAtIndex(_index);
        var element = _prop.GetArrayElementAtIndex(_index);

        element.FindPropertyRelative("Key").stringValue = _data.Key;
        element.FindPropertyRelative("BindingObject").objectReferenceValue = _data.BindingObject;
    }

    private void CopyToClipboard()
    {
        m_StringBuilder
            .Append("public enum Binding")
            .Append("{");

        for (int i = 0; i < m_PropBindingObjects.arraySize; ++i)
        {
            var propBindingObject = m_PropBindingObjects.GetArrayElementAtIndex(i);
            string key = propBindingObject.FindPropertyRelative("Key").stringValue;

            m_StringBuilder.Append($"{key},\n");
        }
        m_StringBuilder.Append("}");

        GUIUtility.systemCopyBuffer = m_StringBuilder.ToString();
    }
}

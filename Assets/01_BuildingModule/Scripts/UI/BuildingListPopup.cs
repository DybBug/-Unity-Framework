using BuildingModule;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingListPopup : MonoBehaviour
{
    private static Timer _Timer;
    private enum State
    {
        WaitingForStart,
        InProgress,
        WaitingForCompletion
    }

    [Header("-- Element Prefab --")]
    [SerializeField] private BuildingListElement m_ElementPrefab;

    [Header("-- Top --")]
    [SerializeField] private Button m_CloseButton;

    [Header("-- Middle --")]
    [SerializeField] private RectTransform m_Content;

    [Header("-- Bottom --")]
    [SerializeField] private Button m_CreateButton;

    private BuildingListElement m_SelectedElement;

    private void Awake()
    {
        m_CloseButton.onClick.AddListener(OnClickedCloseButton);
        m_CreateButton.onClick.AddListener(OnClickedCreateButton);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var key in BuildingTable.Instance.GetKeys())
        {
            var element = Instantiate(m_ElementPrefab);
            element.transform.SetParent(m_Content.transform, false);
            element.Setup(BuildingTable.Instance.GetItemByKey(key));
            element.OnClickedEvent += OnSelectedElement;
            element.SetHighlight(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Open()
    {
        gameObject.SetActive(true);;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnClickedCloseButton()
    {
        Close();
    }

    private void OnClickedCreateButton()
    {
        if (m_SelectedElement == null)
            return;

        Building.Instantiate(m_SelectedElement.Item.Key);
        Close();
    }

    private void OnSelectedElement(BuildingListElement element, string key)
    {
        if (m_SelectedElement != null)
        {
            m_SelectedElement.SetHighlight(false);
        }

        m_SelectedElement = element;
        m_SelectedElement.SetHighlight(true);
    }
}

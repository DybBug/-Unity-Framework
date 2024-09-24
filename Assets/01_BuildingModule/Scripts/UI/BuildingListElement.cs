using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildingListElement : MonoBehaviour
{
    [SerializeField] private Button m_Button;
    [SerializeField] private Image m_IconImage;
    [SerializeField] private TMP_Text m_NameText;
    [SerializeField] private TMP_Text m_DescText;
    [SerializeField] private TMP_Text m_TimeText;

    public BuildingTable.Item Item { get; private set; }

    public event UnityAction<BuildingListElement, string/*key*/> OnClickedEvent;

    private void Awake()
    {
        m_Button.onClick.AddListener(OnClickedElement);
    }

    public void Setup(BuildingTable.Item item)
    {
        Item = item;

        m_IconImage.sprite = Item.Sprite;
        m_NameText.text = Item.Name;
        m_DescText.text = Item.Description;
        m_TimeText.text = $"{Item.TakeTimeSec} sec";
    }

    public void SetHighlight(bool isHighlight)
    {

    }

    private void OnClickedElement() => OnClickedEvent?.Invoke(this, Item.Key);
}

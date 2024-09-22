using UnityEngine;
using UnityEngine.Events;

public class Bubble : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_BackgroundSpriteRenderer;
    [SerializeField] private SpriteRenderer m_ImageSpriteRenderer;

    private Color m_DefaultBackgroundColor;
    private Color m_DefaultImageColor;

    public bool IsShow { get; private set; } = true;
    public bool IsActivate { get; private set; } = true;

    private event UnityAction m_OnClicked;

    #region Unity

    private void Awake()
    {
        m_DefaultBackgroundColor = m_BackgroundSpriteRenderer.color;
        m_DefaultImageColor = m_ImageSpriteRenderer.color;
    }

    private void Start()
    {
        var interact = GetComponent<Interact>();
        interact.Register(new InteractEventListener()
        {
            OnInteractBeginEvent = null,
            OnInteractingEvent = null,
            OnInteractEndEvent = OnInteractEnd
        });
    }

    private void OnDestroy()
    {
        UnregisterOnClickedListener();
    }
    #endregion

    public void RegisterOnClickedListener(UnityAction onClickedListener) => m_OnClicked = onClickedListener;
    public void UnregisterOnClickedListener() => m_OnClicked = null;
    public void SetBackgroundSprite(Sprite sprite) => m_BackgroundSpriteRenderer.sprite = sprite;
    public void SetBackgroundColor(Color color) => m_BackgroundSpriteRenderer.color = color;
    public void SetImageSprite(Sprite sprite) => m_ImageSpriteRenderer.sprite = sprite;

    private void OnInteractEnd(InteractEventParam arg0)
    {
        if (!IsActivate)
            return;

        m_OnClicked?.Invoke();
    }

    public void Show() 
    {
        IsShow = true;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        IsShow = false;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        if (IsActivate)
            return;

        IsActivate = true;
        m_BackgroundSpriteRenderer.color = m_DefaultBackgroundColor;
        m_ImageSpriteRenderer.color = m_DefaultImageColor;
    }

    public void Deactivate()
    {
        if (!IsActivate)
            return;

        IsActivate = false;
        var color = m_DefaultBackgroundColor * 0.5f;
        color.a = 1.0f;
        m_BackgroundSpriteRenderer.color = color;

        color = m_DefaultImageColor * 0.5f;
        color.a = 1.0f;
        m_ImageSpriteRenderer.color = color;
    }
}
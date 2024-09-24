using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Building : MonoBehaviour
{
    public enum State
    {
        Idle,
        Relocation,
        Construction,
    }

    [SerializeField] private BoundsInt m_Area;
    public BoundsInt Area { get => m_Area; set => m_Area = value; }

    [SerializeField] private Tilemap m_Tilemap;
    [SerializeField] private TilemapRenderer m_TilemapRenderer;
    [SerializeField] private Bubble m_TopBubble;
    [SerializeField] private Bubble m_BottomBubble;
    [SerializeField] private SpriteRenderer m_StateSpriteRenderer;

    public event UnityAction<InteractEventParam> OnSelectedEvent;
    public event UnityAction<InteractEventParam> OnDragEvent;
    public event UnityAction<InteractEventParam> OnUnselectedEvent;
    public event UnityAction<InteractEventParam> OnPressedEvent;

    public event UnityAction OnTopBubbleClickedEvent;
    public event UnityAction OnBottomBubbleClickedEvent;

    private long m_PressedStartTimeMs;
    private long m_PressedAccTimeMs;
    private const long PRESSED_TIME_MS_TO_RELOCATION = 1000;


    private readonly Dictionary<Building.State, BuildingState> m_StateMap = new();
    public BuildingState CurrentState { get; private set; }
    public string Key { get; private set; }
    public string Name => BuildingTable.Instance.GetItemByKey(Key).Name;
    public string Description => BuildingTable.Instance.GetItemByKey(Key).Description;
    public long ProcessingDurationMs => BuildingTable.Instance.GetItemByKey(Key).TakeTimeSec * 1000;
    public long ProcessingStartTimeMs { get; private set; }
    public long ProcessingEndTimeMs => ProcessingStartTimeMs + ProcessingDurationMs;

    private BuildingState.Param m_StateParam = new();

    #region Unity
    private void Awake()
    {
        RegisterState(State.Idle, new BuildingState_Idle(this, m_StateParam));
        RegisterState(State.Relocation, new BuildingState_Relocation(this, m_StateParam));
        RegisterState(State.Construction, new BuildingState_Construction(this, m_StateParam));

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var interactObject = GetComponent<Interact>();
        interactObject.OnInteractBeginEvent += OnInteractBegin;
        interactObject.OnDragEvent += OnDrag;
        interactObject.OnInteractEndEvent += OnInteractEnd;
        interactObject.OnPressEvent += OnPress;

        m_TopBubble.OnClickedEvent += OnTopBubbleClicked;
        m_BottomBubble.OnClickedEvent += OnBottomBubbleClicked;
    }

    private void OnDestroy()
    {
        var interactObject = GetComponent<Interact>();
        interactObject.OnInteractBeginEvent -= OnInteractBegin;
        interactObject.OnDragEvent -= OnDrag;
        interactObject.OnInteractEndEvent -= OnInteractEnd;
        interactObject.OnPressEvent -= OnPress;

        m_TopBubble.OnClickedEvent -= OnTopBubbleClicked;
        m_BottomBubble.OnClickedEvent -= OnBottomBubbleClicked;
    }

    #endregion

    public void TransitionState(Building.State newState)
    {
        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        CurrentState = m_StateMap[newState];
        CurrentState.Enter();
    }

    public void SetTileColor(Color color) => m_Tilemap.color = color;
    public void ShowTile() => m_TilemapRenderer.enabled = true;
    public void HideTile() => m_TilemapRenderer.enabled = false;
    public void ActivateBottomBubble() => m_BottomBubble.Activate();
    public void DeactivateBottomBubble() => m_BottomBubble.Deactivate();

    // state
    public void SetStateSprite(Sprite sprite) => m_StateSpriteRenderer.sprite = sprite;
    public void ShowStateSprite() => m_StateSpriteRenderer.transform.parent.gameObject.SetActive(true);
    public void HideStateSprite() => m_StateSpriteRenderer.transform.parent.gameObject.SetActive(false);

    // top bubble;
    public void SetTopBubbleSprite(Sprite sprite) => m_TopBubble.SetImageSprite(sprite);
    public void ShowTopBubble() => m_TopBubble.Show();
    public void HideTopBubble() => m_TopBubble.Hide();

    // bottom bubble
    public void SetBottomBubbleSprite(Sprite sprite) => m_BottomBubble.SetImageSprite(sprite);
    public void ShowBottomBubble() => m_BottomBubble.Show();
    public void HideBottomBubble() => m_BottomBubble.Hide();


    public void Place()
    {
        HideTile();
        HideBottomBubble();
    }

    public void Pickup()
    {
        ShowTile();
        ShowBottomBubble();
    }

    public void Buildable()
    {
        var color = Color.green;
        color.a *= 0.5f;
        SetTileColor(color);
        ActivateBottomBubble();
    }

    public void Unbuildable()
    {
        var color = Color.red;
        color.a *= 0.5f;
        SetTileColor(color);
        DeactivateBottomBubble();
    }

    protected void RegisterState(Building.State state, BuildingState buildingState) => m_StateMap.Add(state, buildingState);
    protected void OnBottomBubbleClicked() => OnBottomBubbleClickedEvent?.Invoke();
    protected void OnTopBubbleClicked() => OnTopBubbleClickedEvent?.Invoke();

    protected bool IsActivatePress => m_PressedStartTimeMs > 0;

    #region InteractObject
    protected void OnInteractBegin(InteractEventParam param)
    {
        m_PressedStartTimeMs = TimeUtil.NowUtc(false);
        OnSelectedEvent?.Invoke(param);
    }

    protected void OnDrag(InteractEventParam param)
    {
        OnDragEvent?.Invoke(param);
    }

    protected void OnInteractEnd(InteractEventParam param)
    {
        m_PressedStartTimeMs = 0;
        OnUnselectedEvent?.Invoke(param);
    }

    protected void OnPress(InteractEventParam param)
    {
        if (!IsActivatePress)
            return;

        m_PressedAccTimeMs = TimeUtil.NowUtc(false) - m_PressedStartTimeMs;
        if (m_PressedAccTimeMs >= PRESSED_TIME_MS_TO_RELOCATION)
        {
            OnPressedEvent?.Invoke(param);
            m_PressedAccTimeMs = 0;
            m_PressedStartTimeMs = 0;
        }
    }
    #endregion

    public static Building Instantiate(string key)
    {
        var tableItem = BuildingTable.Instance.GetItemByKey(key);
        var building = Instantiate(tableItem.Prefab, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        building.Key = key;

        building.ProcessingStartTimeMs = TimeUtil.NowUtc(false);
        building.m_StateParam.isConstructionCompleted = false;
        building.TransitionState(State.Relocation);

        building.HideStateSprite();

        return building;
    }
}

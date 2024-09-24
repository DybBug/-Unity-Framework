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
    [SerializeField] private Bubble m_PlaceConfirmBubble;
    [SerializeField] private SpriteRenderer m_StateSpriteRenderer;

    public event UnityAction<InteractEventParam> OnSelectedEvent;
    public event UnityAction<InteractEventParam> OnDragEvent;
    public event UnityAction<InteractEventParam> OnUnselectedEvent;

    public event UnityAction OnTopBubbleClickedEvent;
    public event UnityAction OnPlaceConfirmBubbleClickedEvent;


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
        interactObject.OnInteractingEvent += OnInteracting;
        interactObject.OnInteractEndEvent += OnInteractEnd;

        m_TopBubble.OnClickedEvent += OnTopBubbleClicked;
        m_PlaceConfirmBubble.OnClickedEvent += OnPlaceConfirmBubbleClicked;
    }

    private void OnDestroy()
    {
        var interactObject = GetComponent<Interact>();
        interactObject.OnInteractBeginEvent -= OnInteractBegin;
        interactObject.OnInteractingEvent -= OnInteracting;
        interactObject.OnInteractEndEvent -= OnInteractEnd;

        m_TopBubble.OnClickedEvent -= OnTopBubbleClicked;
        m_PlaceConfirmBubble.OnClickedEvent -= OnPlaceConfirmBubbleClicked;
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
    public void ShowPlaceConfirmBubble() => m_PlaceConfirmBubble.Show();
    public void HidePlaceConfirmBubble() => m_PlaceConfirmBubble.Hide();
    public void ActivatePlaceConfirmBubble() => m_PlaceConfirmBubble.Activate();
    public void DeactivatePlaceConfirmBubble() => m_PlaceConfirmBubble.Deactivate();
    public void SetStateSprite(Sprite sprite) => m_StateSpriteRenderer.sprite = sprite;
    public void ShowStateSprite() => m_StateSpriteRenderer.transform.parent.gameObject.SetActive(true);
    public void HideStateSprite() => m_StateSpriteRenderer.transform.parent.gameObject.SetActive(false);
    public void SetTopBubbleSprite(Sprite sprite) => m_TopBubble.SetImageSprite(sprite);
    public void ShowTopBubble() => m_TopBubble.Show();
    public void HideTopBubble() => m_TopBubble.Hide();


    public void Place()
    {
        HideTile();
        HidePlaceConfirmBubble();
    }

    public void Pickup()
    {
        ShowTile();
        ShowPlaceConfirmBubble();
    }

    public void Buildable()
    {
        var color = Color.green;
        color.a *= 0.5f;
        SetTileColor(color);
        ActivatePlaceConfirmBubble();
    }

    public void Unbuildable()
    {
        var color = Color.red;
        color.a *= 0.5f;
        SetTileColor(color);
        DeactivatePlaceConfirmBubble();
    }

    protected void RegisterState(Building.State state, BuildingState buildingState) => m_StateMap.Add(state, buildingState);
    protected void OnPlaceConfirmBubbleClicked() => OnPlaceConfirmBubbleClickedEvent?.Invoke();
    protected void OnTopBubbleClicked() => OnTopBubbleClickedEvent?.Invoke();

    #region InteractObject
    protected void OnInteractBegin(InteractEventParam param) => OnSelectedEvent?.Invoke(param);
    protected void OnInteracting(InteractEventParam param) => OnDragEvent?.Invoke(param);
    protected void OnInteractEnd(InteractEventParam param) => OnUnselectedEvent?.Invoke(param);
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

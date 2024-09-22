using BuildingModule;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Building : MonoBehaviour
{
    public bool IsPlaced { get; private set; }

    [SerializeField]private BoundsInt m_Area;
    public BoundsInt Area { get => m_Area; set => m_Area = value; }

    [SerializeField] private Tilemap m_Tilemap;

    [SerializeField] private TilemapRenderer m_TilemapRenderer;

    [SerializeField] private Bubble m_PlaceConfirmBubble;

    private bool m_IsBuildable;

    #region Unity
    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var interactObject = GetComponent<Interact>();
        interactObject.Register(new InteractEventListener()
        {
            OnInteractBeginEvent = OnInteractBegin,
            OnInteractEndEvent = OnInteractEnd,
            OnInteractingEvent = OnInteracting
        });

        m_PlaceConfirmBubble.RegisterOnClickedListener(OnPlaceConfirmBubbleClicked);
    }

    private void OnDestroy()
    {
        m_PlaceConfirmBubble.UnregisterOnClickedListener();
    }

    #endregion

    public void Place()
    {
        BuildingSystem.Instance.UnregisterBuildableEventListener();
        BuildingSystem.Instance.UnregisterUnbuildableEventListener();

        HideTile();
        IsPlaced = true;
        m_PlaceConfirmBubble.Hide();
    }

    public void Pickup()
    {
        BuildingSystem.Instance.RegisterBuildableEventListener(OnBuildable);
        BuildingSystem.Instance.RegisterUnbuildableEventListener(OnUnbuildable);
        ShowTile();
        m_PlaceConfirmBubble.Show();
        IsPlaced = false;
    }

    private void OnBuildable()
    {
        m_IsBuildable = true;
        var color = Color.green;
        color.a *= 0.5f;
        SetTileColor(color);

        m_PlaceConfirmBubble.Activate();
    }

    private void OnUnbuildable()
    {
        m_IsBuildable = false;
        var color = Color.red;
        color.a *= 0.5f;
        SetTileColor(color);

        m_PlaceConfirmBubble.Deactivate();
    }

    public void SetTileColor(Color color) => m_Tilemap.color = color;
    public void ShowTile() => m_TilemapRenderer.enabled = true;
    public void HideTile() => m_TilemapRenderer.enabled = false;

    private void OnPlaceConfirmBubbleClicked()
    {
        BuildingSystem.Instance.Place();
    }


    #region InteractObject
    private void OnInteractBegin(InteractEventParam param)
    {
        BuildingSystem.Instance.TryPickup(this);
    }

    private void OnInteractEnd(InteractEventParam param)
    {

    }

    private void OnInteracting(InteractEventParam param)
    {
        BuildingSystem.Instance.FollowBuilding(param.mouseWorldPose);
    }
    #endregion
}

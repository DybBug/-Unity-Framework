using BuildingModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Building : MonoBehaviour
{

    [SerializeField]private BoundsInt m_Area;
    public BoundsInt Area { get => m_Area; set => m_Area = value; }

    [SerializeField] private Tilemap m_Tilemap;

    [SerializeField] private TilemapRenderer m_TilemapRenderer;

    [SerializeField] private Bubble m_PlaceConfirmBubble;

    private readonly Dictionary<BuildingStrategy.Type, BuildingStrategy> m_StrategiesByType = new();

    #region Unity
    private void Awake()
    {
        RegisterStrategy(new BuildingRelocation(this));
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

    private void RegisterStrategy(BuildingStrategy strategy) => m_StrategiesByType.Add(strategy.StrategyType, strategy);
    public BuildingStrategy GetStrategy(BuildingStrategy.Type type) => m_StrategiesByType[type];
    public void SetTileColor(Color color) => m_Tilemap.color = color;
    public void ShowTile() => m_TilemapRenderer.enabled = true;
    public void HideTile() => m_TilemapRenderer.enabled = false;
    public void ShowPlaceConfirmBubble() => m_PlaceConfirmBubble.Show();
    public void HidePlaceConfirmBubble() => m_PlaceConfirmBubble.Hide();
    public void ActivatePlaceConfirmBubble() => m_PlaceConfirmBubble.Activate();
    public void DeactivatePlaceConfirmBubble() => m_PlaceConfirmBubble.Deactivate();

    private void OnPlaceConfirmBubbleClicked()
    {
        GetStrategy(BuildingStrategy.Type.BuildingRelocation).Exit();
    }


    #region InteractObject
    private void OnInteractBegin(InteractEventParam param)
    {
        GetStrategy(BuildingStrategy.Type.BuildingRelocation).Enter();
    }

    private void OnInteractEnd(InteractEventParam param)
    {

    }

    private void OnInteracting(InteractEventParam param)
    {
        GetStrategy(BuildingStrategy.Type.BuildingRelocation).Update();
    }
    #endregion
}

using UnityEngine;
using UnityEngine.Tilemaps;

public class Building : MonoBehaviour
{
    public bool IsPlaced { get; private set; }

    [SerializeField]
    private BoundsInt m_Area;
    public BoundsInt Area { get => m_Area; set => m_Area = value; }

    [SerializeField]
    private Tilemap m_Tilemap;

    [SerializeField]
    private TilemapRenderer m_TilemapRenderer;

    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Place()
    {
        IsPlaced = true;
    }

    public void Pickup()
    {
        IsPlaced = false;
    }

    public void SetTileColor(Color color) => m_Tilemap.color = color;
    public void ShowTile() => m_TilemapRenderer.gameObject.SetActive(true);
    public void HideTile() => m_TilemapRenderer.gameObject.SetActive(false);
}

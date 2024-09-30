using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

namespace BuildingModule
{
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance;

        [SerializeField] private GridLayout m_GridLayout;
        [SerializeField] private Tilemap m_MainTileMap;
        [SerializeField] private Tilemap m_BuildingTileMap;
        [SerializeField] private TileBase m_BuildingTile;
        
        private Building m_PickedBuilding;
        private Vector3 m_PrevCellPosition;


        #region Unity Method
        private void Awake()
        {
            Instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        #endregion

        public bool TryPickup(Building building)
        {
            if (m_PickedBuilding != null)
                return false;

            m_PickedBuilding = building;

            var position = m_GridLayout.LocalToCell(m_PickedBuilding.transform.position);
            var copyArea = m_PickedBuilding.Area;
            copyArea.position = position;
            m_PickedBuilding.Pickup();

            var tiles = m_BuildingTileMap.GetTilesBlock(copyArea);
            for (var i = 0; i < tiles.Length; ++i)
            {
                tiles[i] = null;
            }
            m_BuildingTileMap.SetTilesBlock(copyArea, tiles);
            CheckBuildableCell(m_PickedBuilding);
            return true;
        }


        public void FollowBuilding(Vector3 mouseWorldPos)
        {
            if (m_PickedBuilding == null)
                return;

            var cellPos = m_GridLayout.LocalToCell(mouseWorldPos);
            cellPos.z = 0;

            if (m_PrevCellPosition == cellPos)
                return;

            m_PrevCellPosition = cellPos;
            m_PickedBuilding.transform.localPosition = m_GridLayout.CellToLocal(cellPos);

            CheckBuildableCell(m_PickedBuilding);
        }

        public void Place()
        {
            if (m_PickedBuilding == null)
                return;

            var position = m_GridLayout.LocalToCell(m_PickedBuilding.transform.position);
            var copyArea = m_PickedBuilding.Area;
            copyArea.position = position;
            m_PickedBuilding.Place();

            var tiles = m_BuildingTileMap.GetTilesBlock(copyArea);
            for (var i = 0; i < tiles.Length; ++i)
            {
                tiles[i] = m_BuildingTile;
            }
            m_BuildingTileMap.SetTilesBlock(copyArea, tiles);
            m_PickedBuilding = null;
        }

        public Vector3Int ConvertWorldToCell(Vector3 pos)
        {
            pos.z = 0;
            return m_GridLayout.WorldToCell(pos);
        }

        public Vector3 ConvertCellToWorld(Vector3Int cell)
        {
            cell.z = 0;
            var worldPos = m_GridLayout.CellToWorld(cell);
            if (m_GridLayout.cellLayout == GridLayout.CellLayout.Isometric)
            {
                worldPos.y += m_GridLayout.cellSize.y * 0.5f;
            }
            return worldPos;
        }

        public CustomTile FindTileOrNull(Vector3 pos)
        {
            return FindTileOrNull(ConvertWorldToCell(pos));
        }

        public CustomTile FindTileOrNull(int column, int row)
        {
            return FindTileOrNull(new Vector3Int(column, row));
        }

        public CustomTile.TileType GetTileType(int column, int row)
        {
            var cell = new Vector3Int(column, row);
            var tile = m_BuildingTileMap.GetTile<CustomTile>(cell);
            if (tile != null)
            {
                return tile.Type;
            }

            tile = m_MainTileMap.GetTile<CustomTile>(cell);
            if (tile != null)
            {
                return tile.Type;
            }
            return CustomTile.TileType.None;
        }

        private bool CheckBuildableCell(Building Building)
        {
            var buildingArea = Building.Area;
            buildingArea.position = m_GridLayout.WorldToCell(m_PickedBuilding.transform.position);

            var mainTilesInArea = m_MainTileMap.GetTilesBlock(buildingArea);
            for (var i = 0; i < mainTilesInArea.Length; ++i)
            {
                var mainTileAsCustomTile = mainTilesInArea[i] as CustomTile;
                if (mainTileAsCustomTile == null || mainTileAsCustomTile.Type == CustomTile.TileType.Block)
                {
                    Building.Unbuildable();
                    return false;
                }
            }
            var buildingTilesInArea = m_BuildingTileMap.GetTilesBlock(buildingArea);
            for (var i = 0; i < buildingTilesInArea.Length; ++i)
            {
                if (buildingTilesInArea[i] != null)
                {
                    Building.Unbuildable();
                    return false;
                }
            }
            Building.Buildable();
            return true;
        }

        private bool CanBePlaced(BuildingState_Relocation BuildingRelocation)
        {
            var position = m_GridLayout.LocalToCell(BuildingRelocation.Owner.transform.position);
            var copyArea = BuildingRelocation.Owner.Area;
            copyArea.position = position;

            var mainTiles = m_MainTileMap.GetTilesBlock(copyArea);
 
            return mainTiles.Select(e => e as CustomTile).Any(e => e == null || e.Type == CustomTile.TileType.Block) ? false : true;
        }

        private CustomTile FindTileOrNull(Vector3Int cell)
        {
            var tile = m_BuildingTileMap.GetTile<CustomTile>(cell);
            if (tile == null)
            {
                tile = m_MainTileMap.GetTile<CustomTile>(cell);
            }
            return tile;
        }

    }
}

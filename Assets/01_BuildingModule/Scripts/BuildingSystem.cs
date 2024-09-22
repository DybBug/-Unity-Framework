using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        public void InitializeWithBuilding(GameObject buildingPrefab)
        {
            var building = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity).GetComponent<Building>();
            CheckBuildableCell(building);
        }

        public void Pickup(Building building)
        {
            Debug.Assert(building != null);

            var position = m_GridLayout.LocalToCell(building.transform.position);
            var copyArea = building.Area;
            copyArea.position = position;
            building.Pickup();

            var tiles = m_BuildingTileMap.GetTilesBlock(copyArea);
            for (var i = 0; i < tiles.Length; ++i)
            {
                tiles[i] = null;
            }
            m_BuildingTileMap.SetTilesBlock(copyArea, tiles);

            m_PickedBuilding = building;
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

        private bool CheckBuildableCell(Building building)
        {
            var tileColor = Color.green;
            tileColor.a = 0.5f;

            var buildingArea = building.Area;
            buildingArea.position = m_GridLayout.WorldToCell(building.transform.position);

            var mainTilesInArea = m_MainTileMap.GetTilesBlock(buildingArea);
            for (var i = 0; i < mainTilesInArea.Length; ++i)
            {
                var mainTileAsCustomTile = mainTilesInArea[i] as CustomTile;
                if (mainTileAsCustomTile == null || mainTileAsCustomTile.Type == CustomTile.TileType.Block)
                {
                    tileColor = Color.red;
                    tileColor.a = 0.5f;
                    building.SetTileColor(tileColor);
                    return false;
                }
            }
            var buildingTilesInArea = m_BuildingTileMap.GetTilesBlock(buildingArea);
            for (var i = 0; i < buildingTilesInArea.Length; ++i)
            {
                if (buildingTilesInArea[i] != null)
                {
                    tileColor = Color.red;
                    tileColor.a = 0.5f;
                    building.SetTileColor(tileColor);
                    return false;
                }
            }

            building.SetTileColor(tileColor);
            return true;
        }

        private bool CanBePlaced(Building building)
        {
            Debug.Assert(building != null);

            var position = m_GridLayout.LocalToCell(building.transform.position);
            var copyArea = building.Area;
            copyArea.position = position;

            var mainTiles = m_MainTileMap.GetTilesBlock(copyArea);
 
            return mainTiles.Select(e => e as CustomTile).Any(e => e == null || e.Type == CustomTile.TileType.Block) ? false : true;
        }
    }
}

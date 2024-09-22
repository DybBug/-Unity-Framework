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
        
        private BuildingRelocation m_PickedBuildingRelocation;
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

        public void CreateBuilding(GameObject buildingPrefab)
        {
            var building = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity).GetComponent<Building>();
            var BuildingRelocation = building.GetStrategy(BuildingStrategy.Type.BuildingRelocation) as BuildingRelocation;
            if (TryPickup(BuildingRelocation))
            {
                CheckBuildableCell(BuildingRelocation);
            }
        }

        public bool TryPickup(BuildingRelocation BuildingRelocation)
        {
            if (m_PickedBuildingRelocation != null)
                return false;

            m_PickedBuildingRelocation = BuildingRelocation;

            var position = m_GridLayout.LocalToCell(m_PickedBuildingRelocation.OwnerBuilding.transform.position);
            var copyArea = m_PickedBuildingRelocation.OwnerBuilding.Area;
            copyArea.position = position;
            m_PickedBuildingRelocation.Pickup();

            var tiles = m_BuildingTileMap.GetTilesBlock(copyArea);
            for (var i = 0; i < tiles.Length; ++i)
            {
                tiles[i] = null;
            }
            m_BuildingTileMap.SetTilesBlock(copyArea, tiles);
            return true;
        }


        public void FollowBuilding(Vector3 mouseWorldPos)
        {
            if (m_PickedBuildingRelocation == null)
                return;

            var cellPos = m_GridLayout.LocalToCell(mouseWorldPos);
            cellPos.z = 0;

            if (m_PrevCellPosition == cellPos)
                return;

            m_PrevCellPosition = cellPos;
            m_PickedBuildingRelocation.OwnerBuilding.transform.localPosition = m_GridLayout.CellToLocal(cellPos);

            CheckBuildableCell(m_PickedBuildingRelocation);
        }

        public void Place()
        {
            if (m_PickedBuildingRelocation == null)
                return;

            var position = m_GridLayout.LocalToCell(m_PickedBuildingRelocation.OwnerBuilding.transform.position);
            var copyArea = m_PickedBuildingRelocation.OwnerBuilding.Area;
            copyArea.position = position;
            m_PickedBuildingRelocation.Place();

            var tiles = m_BuildingTileMap.GetTilesBlock(copyArea);
            for (var i = 0; i < tiles.Length; ++i)
            {
                tiles[i] = m_BuildingTile;
            }
            m_BuildingTileMap.SetTilesBlock(copyArea, tiles);
            m_PickedBuildingRelocation = null;
        }

        private bool CheckBuildableCell(BuildingRelocation BuildingRelocation)
        {
            var buildingArea = BuildingRelocation.OwnerBuilding.Area;
            buildingArea.position = m_GridLayout.WorldToCell(BuildingRelocation.OwnerBuilding.transform.position);

            var mainTilesInArea = m_MainTileMap.GetTilesBlock(buildingArea);
            for (var i = 0; i < mainTilesInArea.Length; ++i)
            {
                var mainTileAsCustomTile = mainTilesInArea[i] as CustomTile;
                if (mainTileAsCustomTile == null || mainTileAsCustomTile.Type == CustomTile.TileType.Block)
                {
                    BuildingRelocation.Unbuildable();
                    return false;
                }
            }
            var buildingTilesInArea = m_BuildingTileMap.GetTilesBlock(buildingArea);
            for (var i = 0; i < buildingTilesInArea.Length; ++i)
            {
                if (buildingTilesInArea[i] != null)
                {
                    BuildingRelocation.Unbuildable();
                    return false;
                }
            }
            BuildingRelocation.Buildable();
            return true;
        }

        private bool CanBePlaced(BuildingRelocation BuildingRelocation)
        {
            var position = m_GridLayout.LocalToCell(BuildingRelocation.OwnerBuilding.transform.position);
            var copyArea = BuildingRelocation.OwnerBuilding.Area;
            copyArea.position = position;

            var mainTiles = m_MainTileMap.GetTilesBlock(copyArea);
 
            return mainTiles.Select(e => e as CustomTile).Any(e => e == null || e.Type == CustomTile.TileType.Block) ? false : true;
        }
    }
}

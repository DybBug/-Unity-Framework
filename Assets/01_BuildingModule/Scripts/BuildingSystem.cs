using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
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
    }
}

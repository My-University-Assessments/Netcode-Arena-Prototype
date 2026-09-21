using System.Collections.Generic;
using ArenaPrototype.Feature.GridSystem.Interface;
using UnityEngine;

namespace ArenaPrototype.Feature.GridSystem
{
    public enum GridTileType
    {
        Pointed = 0,
        Flat = 1,

    }

    [RequireComponent(typeof(Grid))]
    public class GridGenerator : MonoBehaviour
    {
        public static GridGenerator Singleton; // INFO: Singleton
        public Grid m_gridComponent => GetComponent<Grid>();
        public Dictionary<Vector2Int, GameObject> gridTiles { get; private set; } = new();

        [SerializeField] private GridTileType _gridTileType = GridTileType.Pointed;

        [Header("Prefabs")]
        [SerializeField] private Dictionary<GridTileType, Dictionary<TileType, List<GameObject>>> m_gridTilePrefabs = new();


        private void Awake()
        {
            #region Singleton
            if (Singleton == null)
            {
                Singleton = this;

            }
            else
            {
                Destroy(gameObject);

            }
            #endregion

            if (m_gridComponent == null)
            {
                Debug.LogError("Grid component not set!");
                enabled = false;
                return;

            }
        }

        // INFO: Create Grid
        public void CreateGrid(int width, int height, GridLayout.CellLayout gridLayout = default, Vector3 tileSize = default, GridLayout.CellSwizzle cellSwizzle = default, Vector3 cellGap = default)
        {

            // GUARD: Prevent nulls
            if (m_gridTilePrefabs == null) { Debug.LogError("GridTilePrefab not set!"); return; }
            if (tileSize == default) tileSize = Vector3.one;

            if (gridLayout == default)
            {
                gridLayout = GridLayout.CellLayout.Hexagon;
            }

            // INFO: Set orientation based on flat vs pointed
            if (cellSwizzle == default && gridLayout == GridLayout.CellLayout.Hexagon)
            {
                cellSwizzle = _gridTileType == GridTileType.Pointed
                ? GridLayout.CellSwizzle.XYZ  // INFO: Pointed-top
                : GridLayout.CellSwizzle.YXZ; // INFO: Flat-top

            }

            if (gridLayout == GridLayout.CellLayout.Hexagon)
            {
                tileSize += new Vector3(2f, 2f, 0f);

            }
            else
            {
                m_gridComponent.cellGap = cellGap;

            }


            m_gridComponent.cellSize = tileSize;
            m_gridComponent.cellLayout = gridLayout;
            m_gridComponent.cellSwizzle = cellSwizzle;


            // INFO: Create grid
            for (int i = 0; i < width; i++)
            {
                GameObject columnGO = new GameObject($"Column: {i}");
                columnGO.transform.parent = m_gridComponent.transform;

                for (int j = 0; j < height; j++)
                {
                    Vector3 worldPosition = m_gridComponent.GetCellCenterWorld(new Vector3Int(i, j, 0));

                    // INFO: Spawn Prefab
                    GameObject tile = Instantiate(m_gridTilePrefabs[_gridTileType][0][0], new Vector3(worldPosition.x, 0, worldPosition.y), Quaternion.identity);

                    Vector2Int axialCoords = OffsetToAxial(new Vector2Int(i, j));
                    tile.name = $"Tile: {axialCoords} | Row: {j}";
                    tile.transform.parent = columnGO.transform;

                    gridTiles.Add(axialCoords, tile);


                }
            }

        }

        #region Helper
        private int GetAxialDistance(Vector2Int a, Vector2Int b)
        {
            int q1 = a.x, r1 = a.y;
            int q2 = b.x, r2 = b.y;

            return (Mathf.Abs(q1 - q2) + Mathf.Abs(r1 - r2) + Mathf.Abs((q1 + r1) - (q2 + r2))) / 2;
        }

        private Vector2Int OffsetToAxial(Vector2Int offset)
        {
            int col = offset.x;
            int row = offset.y;

            int q = col - (row - (row & 1)) / 2;
            int r = row;
            return new Vector2Int(q, r);

        }

        #endregion

        #region Utility
        private Vector2Int GetGridTileFromWorldPosition(Vector3 worldPosition)
        {
            // Guard: Raycast must hit something
            if (!Physics.Raycast(worldPosition, Vector3.down, out RaycastHit hit))
            {
                Debug.LogWarning("Raycast didn't hit anything!");
                return Vector2Int.zero;
            }

            // Guard: Hit object must be in grid
            if (!gridTiles.ContainsValue(hit.collider.gameObject))
            {
                Debug.LogWarning("Raycast hit object that isn't a grid tile!");
                return Vector2Int.zero;
            }

            // INFO: Find and return the tile coordinates
            foreach (var kvp in gridTiles)
            {
                if (kvp.Value != hit.collider.gameObject) continue;
                return kvp.Key;

            }

            Debug.LogWarning("Tile found in ContainsValue but not in dictionary!");
            return Vector2Int.zero;
        }

        public List<GameObject> GetTilesWithinRadius(Vector3 centrePosition, float radius)
        {
            if (gridTiles == null || gridTiles.Count <= 0)
            {
                Debug.LogError($"No tiles to search!");
                return new();
            }

            List<GameObject> tilesInRadius = new();
            Vector2Int gridCenter = GetGridTileFromWorldPosition(centrePosition);
            int radiusInTiles = Mathf.RoundToInt(radius);

            // INFO: Find Neighbour Tiles
            foreach (var kvp in gridTiles)
            {
                Vector2Int tileCoord = kvp.Key;
                int distance = GetAxialDistance(gridCenter, tileCoord);

                // GUARD: Exclude centre
                if (distance > radiusInTiles || distance <= 0) continue;
                tilesInRadius.Add(kvp.Value);

            }

            return tilesInRadius;
        }

        public void RegenerateGrid(int gridColumns, int gridRows, GridLayout.CellLayout gridLayout = default, Vector2 tileSize = default, GridLayout.CellSwizzle cellSwizzle = default)
        {
            ClearGrid();
            CreateGrid(gridColumns, gridRows);

        }

        private void ClearGrid()
        {
            foreach (GameObject tile in gridTiles.Values)
            {
                Destroy(tile);
                Destroy(tile.transform.parent.gameObject);

            }

            gridTiles = null;

        }

        #endregion

    }
}
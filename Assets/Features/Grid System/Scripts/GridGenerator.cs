using System.Collections.Generic;
using ArenaPrototype.Feature.GridSystem.Interface;
using UnityEngine;
using WebSocketSharp;

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
        public Dictionary<Vector3Int, GameObject> gridTiles { get; private set; } = new();

        [SerializeField] private GridTileType m_gridTileType = GridTileType.Pointed;

        [Header("Prefabs")]
        [SerializeField] private Dictionary<TileType, List<GameObject>> m_gridTilePrefabs = new();


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
        public void CreateGrid(Vector2Int boardSize, GridLayout.CellLayout gridLayout = GridLayout.CellLayout.Rectangle, Vector3 tileSize = default, GridLayout.CellSwizzle cellSwizzle = GridLayout.CellSwizzle.XYZ, Vector3 cellGap = default)
        {

            // GUARD: Prevent nulls
            if (m_gridTilePrefabs == null) { Debug.LogError("GridTilePrefab not set!"); return; }
            if (tileSize == default) tileSize = Vector3.one;

            // INFO: Set orientation based on flat vs pointed
            if (gridLayout == GridLayout.CellLayout.Hexagon)
            {
                cellSwizzle = m_gridTileType == GridTileType.Pointed
                ? GridLayout.CellSwizzle.XYZ  // INFO: Pointed-top
                : GridLayout.CellSwizzle.YXZ; // INFO: Flat-top

                tileSize += new Vector3(1f, 1f, 0f);

            }
            else
            {
                m_gridComponent.cellGap = cellGap;

            }


            m_gridComponent.cellSize = tileSize;
            m_gridComponent.cellLayout = gridLayout;
            m_gridComponent.cellSwizzle = cellSwizzle;


            // INFO: Create grid
            for (int i = 0; i < boardSize.x; i++)
            {
                GameObject columnGO = new GameObject($"Column: {i + 1}");
                columnGO.transform.parent = m_gridComponent.transform;

                for (int j = 0; j < boardSize.y; j++)
                {
                    Vector3 worldPosition = m_gridComponent.GetCellCenterWorld(new Vector3Int(i, j, 0));

                    // INFO: Spawn Prefab
                    GameObject tile = Instantiate(m_gridTilePrefabs[0][0], new Vector3(worldPosition.x, 0, worldPosition.y), Quaternion.identity);
                    tile.transform.parent = columnGO.transform;


                    Vector3Int cubeCoords = OddRToCube(new Vector2Int(i, j));
                    tile.name = $"Tile: {new Vector2Int(i, j)}";
                    gridTiles.Add(cubeCoords, tile);


                }
            }

        }

        #region Helper
        private Vector3Int OddRToCube(Vector2Int offset)
        {
            int col = offset.x;
            int row = offset.y;

            int q = col - (row - (row & 1)) / 2;
            int r = row;
            int s = -q - r;

            return new Vector3Int(q, r, s);
        }

        private int GetCubeDistance(Vector3Int a, Vector3Int b)
        {
            int differenceColumn = a.x - b.x;
            int differenceRow = a.y - b.y;
            int differenceDiagonal = a.x + a.y - (b.x + b.y);

            return Mathf.Max(Mathf.Abs(differenceColumn), Mathf.Abs(differenceRow), Mathf.Abs(differenceDiagonal));
        }

        private Vector3Int GetGridTileFromWorldPosition(Vector3 worldPosition)
        {
            // Guard: Raycast must hit something
            if (!Physics.Raycast(worldPosition, Vector3.down, out RaycastHit hit))
            {
                Debug.LogWarning("Raycast didn't hit anything!");
                return Vector3Int.zero;
            }

            // Guard: Hit object must be in grid
            if (!gridTiles.ContainsValue(hit.collider.gameObject))
            {
                Debug.LogWarning("Raycast hit object that isn't a grid tile!");
                return Vector3Int.zero;
            }

            // INFO: Find and return the tile coordinates
            foreach (var kvp in gridTiles)
            {
                if (kvp.Value != hit.collider.gameObject) continue;
                return kvp.Key;

            }

            Debug.LogWarning("Tile found in ContainsValue but not in dictionary!");
            return Vector3Int.zero;
        }

        #endregion

        #region Utility
        public GameObject GetTileAtOffset(int column, int row)
        {
            Vector3Int cubeKey = OddRToCube(new Vector2Int(column, row));

            // GUARD: Ensure the tile actually exists
            if (!gridTiles.TryGetValue(cubeKey, out GameObject tile)) return null;

            return tile;

        }

        public List<IGridTile> GetTilesWithinRadius(Vector3 centrePosition, float radius)
        {
            if (gridTiles == null || gridTiles.Count <= 0)
            {
                Debug.LogError($"No tiles to search!");
                return new();
            }

            List<IGridTile> tilesInRadius = new();
            Vector3Int gridCenter = GetGridTileFromWorldPosition(centrePosition);
            int radiusInTiles = Mathf.RoundToInt(radius);

            // INFO: Find Neighbour Tiles
            foreach (var tile in gridTiles)
            {
                Vector3Int tileCoord = tile.Key;
                int distance = GetCubeDistance(gridCenter, tileCoord);

                // GUARD: Exclude centre
                if (distance > radiusInTiles || distance <= 0) continue;
                tilesInRadius.Add(tile.Value.GetComponent<IGridTile>());

            }
            return tilesInRadius;

        }

        public void RegenerateGrid(Vector2Int boardSize, GridLayout.CellLayout gridLayout = default, Vector3 tileSize = default, GridLayout.CellSwizzle cellSwizzle = default, Vector3 cellGap = default)
        {
            ClearGrid();
            CreateGrid(boardSize, gridLayout, tileSize, cellSwizzle, cellGap);

        }

        private void ClearGrid()
        {
            foreach (GameObject tile in gridTiles.Values)
            {
                Destroy(tile);
                Destroy(tile.transform.parent.gameObject);

            }

            gridTiles.Clear();

        }

        #endregion

    }
}
using System.Collections.Generic;
using ArenaPrototype.Feature.GridSystem.Interface;
using UnityEngine;

namespace ArenaPrototype.Feature.GridSystem
{
    public class GridGenerator : MonoBehaviour
    {
        public static GridGenerator Singleton; // INFO: Singleton
        private Grid m_gridComponent => GetComponent<Grid>();
        public Dictionary<Vector2, GameObject> gridTiles { get; private set; } = new();

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
        public void CreateGrid(int gridColumns, int gridRows, Vector2 tileSize = default)
        {
            // GUARD: Prevent nulls
            if (m_gridTilePrefabs == null) { Debug.LogError("GridTilePrefab not set!"); return; }

            if (tileSize == default) tileSize = new Vector2(1.02f, 1.02f);
            m_gridComponent.cellSize = tileSize;


            // INFO: Create grid
            for (int i = 0; i < gridColumns; i++)
            {
                GameObject columnGO = new GameObject($"Column: {i + 1}");
                columnGO.transform.parent = m_gridComponent.transform;

                for (int j = 0; j < gridRows; j++)
                {
                    Vector3 worldPosition = m_gridComponent.GetCellCenterWorld(new Vector3Int(i, j, 0));
                    worldPosition -= m_gridComponent.GetCellCenterWorld(Vector3Int.zero);

                    // INFO: Spawn Prefab
                    GameObject tile = Instantiate(m_gridTilePrefabs[0][0], worldPosition, Quaternion.identity);

                    tile.name = $"Tile: ({tile.transform.position.x}, {tile.transform.position.z}) | Row: {j + 1}";
                    tile.transform.parent = columnGO.transform;

                    gridTiles.Add(new Vector2(i, j), tile);

                }
            }

        }

        #region Helper

        private TileType GetRandomTileType(TileType min, TileType max) => (TileType)Random.Range((int)min, (int)max + 1);

        #endregion

        #region Utility
        public void RegenerateGrid(int gridColumns, int gridRows)
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
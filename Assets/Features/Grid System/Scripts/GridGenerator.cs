using System.Collections.Generic;
using ArenaPrototype.Feature.GridSystem.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace ArenaPrototype.Feature.GridSystem.Manager
{
    public class GridGenerator : MonoBehaviour
    {
        public static GridGenerator Singleton; // INFO: Singleton
        private Grid m_gridComponent => GetComponent<Grid>();
        private GameObject[,] m_gridArray;

        [Header("Prefabs")]
        [SerializeField] private Dictionary<TileType, List<GameObject>> m_gridTilePrefabs = new();

        [Header("Tile Settings")]
        [SerializeField] Dictionary<TileType, List<Material>> tileMaterials = new();


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
        public void CreateGrid(int gridColumns, int gridRows, Vector3 tileSize = default)
        {
            // GUARD: Prevent nulls
            if (m_gridTilePrefabs == null) { Debug.LogError("GridTilePrefab not set!"); return; }

            if (tileSize == default) tileSize = new Vector3((float)1.05, (float)1.05, 0);
            m_gridComponent.cellSize = tileSize;

            m_gridArray = new GameObject[gridColumns, gridRows]; // INFO: Initialise Array


            // INFO: Create grid
            for (int i = 0; i < m_gridArray.GetLength(0); i++)
            {
                GameObject columnGO = new GameObject($"Column: {i + 1}");
                columnGO.transform.parent = m_gridComponent.transform;

                for (int j = 0; j < m_gridArray.GetLength(1); j++)
                {
                    Vector3 worldPosition = m_gridComponent.GetCellCenterWorld(new Vector3Int(i, j, 0));
                    worldPosition -= m_gridComponent.GetCellCenterWorld(Vector3Int.zero);

                    // INFO: Spawn Prefab
                    GameObject tile = Instantiate(m_gridTilePrefabs[0][0], worldPosition, Quaternion.identity);
                    AssignMaterial(tile);


                    tile.name = $"Tile: ({tile.transform.position.x}, {tile.transform.position.z}) | Row: {j + 1}";
                    tile.transform.parent = columnGO.transform;

                    m_gridArray[i, j] = tile;

                }
            }

        }

        #region Helper
        private void AssignMaterial(GameObject tile, bool randomMaterial = false)
        {
            MeshRenderer meshRenderer = tile.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null) { Debug.LogError($"{tile.name} doesn't have a mesh renderer!"); return; }

            TileType tileType = tile.GetComponent<IGridTile>().tileType;
            int rnd = randomMaterial ? Random.Range(0, tileMaterials[tileType].Count) : 0;

            Material selectedMaterial = tileMaterials[tileType][rnd];
            if (selectedMaterial != null) meshRenderer.material = selectedMaterial;

        }

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
            foreach (GameObject tile in m_gridArray)
            {
                Destroy(tile);
                Destroy(tile.transform.parent.gameObject);


            }

            m_gridArray = null;

        }

        #endregion

    }
}
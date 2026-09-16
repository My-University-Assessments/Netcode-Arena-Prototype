using Unity.VisualScripting;
using UnityEngine;

namespace ArenaPrototype.Feature.GridSystem.Manager
{
    public class GridGenerator : MonoBehaviour
    {
        public static GridGenerator Singleton; // INFO: Singleton

        [Header("Grid Settings")]
        [SerializeField] private Grid gridComponent;
        [SerializeField] private GameObject gridTilePrefab;
        [Space]
        [SerializeField] private int columns = 2;
        [SerializeField] private int rows = 3;

        private GameObject[,] tileArray;

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

        }

        // INFO: Create Grid
        public void CreateGrid(int gridColumns, int gridRows)
        {
            tileArray = new GameObject[gridColumns, gridRows]; // INFO: Initialise Array

            // GUARD: Prevent nulls
            if (gridComponent == null) { Debug.LogError("Grid component not set!"); return; }
            if (gridTilePrefab == null) { Debug.LogError("GridTilePrefab not set!"); return; }

            // INFO: Create grid
            for (int i = 0; i < gridColumns; i++)
            {
                GameObject columnGO = new GameObject($"Column: {i + 1}");
                columnGO.transform.parent = gridComponent.transform;

                for (int j = 0; j < gridRows; j++)
                {
                    Vector3 worldPosition = gridComponent.GetCellCenterWorld(new Vector3Int(i, j, 0));
                    worldPosition -= gridComponent.GetCellCenterWorld(Vector3Int.zero);

                    // INFO: Spawn Prefab
                    GameObject tile = Instantiate(gridTilePrefab, worldPosition, Quaternion.identity);
                    tile.name = $"Tile: ({tile.transform.position.x}, {transform.position.z}) | Row: {j + 1}";
                    tile.transform.parent = columnGO.transform;
                    tileArray[i, j] = tile;

                }
            }
        }

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;
            if (gridComponent == null) return;

            Gizmos.color = new Color(0, 1, 0, 1f);
            float radius = 0.5f;

            // INFO: Draw outline of how the grid will look
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Vector3 pos = gridComponent.GetCellCenterWorld(new Vector3Int(i, j, 0));
                    DrawHexagonOutline(pos, radius);
                }
            }
        }

        private void DrawHexagonOutline(Vector3 center, float radius)
        {
            Vector3[] vertices = new Vector3[6];

            for (int i = 0; i < 6; i++)
            {
                float angle = (i * 60f + 30f) * Mathf.Deg2Rad;
                vertices[i] = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            }

            for (int i = 0; i < 6; i++)
            {
                Gizmos.DrawLine(vertices[i], vertices[(i + 1) % 6]);

            }

        }

        #endregion

    }
}
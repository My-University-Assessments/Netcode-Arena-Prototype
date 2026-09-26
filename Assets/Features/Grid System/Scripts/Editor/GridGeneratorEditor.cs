using ArenaPrototype.Feature.GridSystem;
using UnityEditor;
using UnityEngine;

namespace ArenaPrototype.Feature.GridSystem.Utility
{
    [CustomEditor(typeof(GridGenerator))]
    public class GridGeneratorEditor : Editor
    {
        [Header("Tile Settings")]
        private GridTileType _gridTileType = GridTileType.Pointed;

        [Header("Grid Settings")]
        private GridLayout.CellLayout m_gridLayout = GridLayout.CellLayout.Hexagon;
        private Vector3 _tileSize = Vector3.one;
        private Vector3 _cellGap = Vector3.zero;
        private GridLayout.CellSwizzle _cellSwizzle = GridLayout.CellSwizzle.XYZ;

        [Header("Board Settings")]
        private Vector2Int _boardSize = new(10, 10);

        private int m_buttonHeight = 20;
        private int m_totalTiles;

        public override void OnInspectorGUI()
        {

            if (!Application.isPlaying)
            {
                base.OnInspectorGUI();
                return;
            }

            GridGenerator grid = (GridGenerator)target;
            m_totalTiles = grid.gridTiles.Count;

            EditorGUILayout.LabelField("Board Settings", EditorStyles.boldLabel);
            _boardSize = EditorGUILayout.Vector2IntField("Board Size", _boardSize);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

            _gridTileType = (GridTileType)EditorGUILayout.EnumPopup("Tile Type", _gridTileType);
            m_gridLayout = (GridLayout.CellLayout)EditorGUILayout.EnumPopup("Grid Layout", m_gridLayout);
            _tileSize = EditorGUILayout.Vector3Field("Tile Size", _tileSize);

            if (m_gridLayout == GridLayout.CellLayout.Hexagon) EditorGUI.BeginDisabledGroup(true);
            _cellGap = EditorGUILayout.Vector3Field("Cell Gap", _cellGap);
            EditorGUI.EndDisabledGroup();


            EditorGUILayout.Space();

            if (m_totalTiles > 0)
            {
                EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Total Tiles", m_totalTiles.ToString());
                EditorGUILayout.Space();

            }

            if (m_totalTiles == 0)
            {
                if (GUILayout.Button("Create Grid", GUILayout.Height(m_buttonHeight)))
                    grid.CreateGrid(_boardSize, m_gridLayout, _tileSize, _cellSwizzle, _cellGap);

            }

            if (m_totalTiles > 0)
            {
                if (GUILayout.Button("Regenerate Grid", GUILayout.Height(m_buttonHeight)))
                    grid.RegenerateGrid(_boardSize, m_gridLayout, _tileSize, _cellSwizzle, _cellGap);

                if (GUILayout.Button("Clear Grid", GUILayout.Height(m_buttonHeight)))
                    grid.RegenerateGrid(Vector2Int.zero);

            }

            EditorGUILayout.Space();

        }
    }
}
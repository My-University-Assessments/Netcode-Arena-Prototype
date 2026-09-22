using System.Collections.Generic;
using System.Linq;
using ArenaPrototype.Feature.GridSystem;
using ArenaPrototype.Feature.GridSystem.Interface;
using TeamBuilder.Agents.Data;
using TeamBuilder.Agents.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace TeamBuilder.Agents.Base
{
    public class Agent : MonoBehaviour, IAgent, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public int team { get; set; }
        private bool hovered;
        public AgentDataSO agentData { get; set; }

        // INFO: Animations
        [Header("Animations")]
        [SerializeField] private Animator _agentAnimator;

        Dictionary<GameObject, Color> _markedTiles = new();


        #region Movement
        public void CalculateMovement()
        {
            switch (agentData.movementType)
            {
                case MovementType.AOE:
                    CalculateAOEMovement();
                    break;
            }

        }

        public bool Move(GameObject location)
        {
            if (!_markedTiles.ContainsKey(location)) return false;
            Debug.Log($"Moving {location}");
            ClearMarkedTiles();
            return true;

        }

        #region AOE
        private void CalculateAOEMovement()
        {
            Debug.Log($"AOE Movement");
            GridGenerator _gridGenerator = GridGenerator.Singleton;
            List<IGridTile> tiles = _gridGenerator.GetTilesWithinRadius(gameObject.transform.position, agentData.movementRadius);


            foreach (IGridTile tile in tiles)
            {
                Renderer renderer = tile.gameObject.GetComponentInChildren<Renderer>();
                Color originalColour = renderer.material.color;
                _markedTiles.Add(tile.gameObject, originalColour);

                renderer.material.color = (tile.isOccupied && tile.occupiedBy.GetComponent<IAgent>().team != team) ? Color.red : Color.blue;

            }

        }
        #endregion

        #endregion

        #region Attack
        public bool Attack(GameObject target)
        {
            if (!_markedTiles.ContainsKey(target)) return false;
            Debug.Log($"Attacking");
            ClearMarkedTiles();
            return true;

        }
        #endregion

        private void Clicked()
        {


        }

        public void Hover()
        {
            hovered = !hovered;
        }

        #region Helper
        public void ClearMarkedTiles()
        {
            if (_markedTiles.Count <= 0) return;
            foreach (var tile in _markedTiles)
            {
                var tileRenderer = tile.Key;
                var originalColour = tile.Value;

                tileRenderer.GetComponentInChildren<Renderer>().material.color = originalColour;

            }

            _markedTiles.Clear();

        }
        #endregion

        #region Pointer Events
        public void OnPointerClick(PointerEventData eventData) => Clicked();
        public void OnPointerEnter(PointerEventData eventData) => Hover();
        public void OnPointerExit(PointerEventData eventData) => Hover();

        #endregion

    }
}
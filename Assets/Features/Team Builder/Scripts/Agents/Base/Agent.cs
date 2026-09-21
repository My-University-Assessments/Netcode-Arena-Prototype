using ArenaPrototype.Feature.GridSystem;
using TeamBuilder.Agents.Data;
using TeamBuilder.Agents.Interface;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamBuilder.Agents.Base
{
    public class Agent : MonoBehaviour, IAgent, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hovered;
        public AgentDataSO agentData { get; set; }

        // INFO: Animations
        [Header("Animations")]
        [SerializeField] private Animator _agentAnimator;

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

        #region AOE
        private void CalculateAOEMovement()
        {
            GridGenerator _gridGenerator = GridGenerator.Singleton;
            Debug.Log($"AOE Movement");

            var tiles = _gridGenerator.GetTilesWithinRadius(transform.position, agentData.movementRadius);

            foreach (GameObject tile in tiles)
            {
                // Debug.Log($"{tile.name}");
                tile.GetComponentInChildren<Renderer>().material.color = Color.blue;

            }

        }
        #endregion

        #endregion

        private void Clicked()
        {
            CalculateMovement();

        }

        public void ToggleHovered()
        {
            hovered = !hovered;
        }

        #region Pointer Events
        public void OnPointerClick(PointerEventData eventData) => Clicked();
        public void OnPointerEnter(PointerEventData eventData) => ToggleHovered();
        public void OnPointerExit(PointerEventData eventData) => ToggleHovered();

        #endregion

    }
}
using System.Collections.Generic;
using ArenaPrototype.Feature.GridSystem;
using ArenaPrototype.Feature.GridSystem.Interface;
using TeamBuilder.Agents.Base;
using TeamBuilder.Agents.Data;
using TeamBuilder.Agents.Interface;
using UnityEngine;

namespace TeamBuilder.Agents.Manager
{
    public class TeamManager : MonoBehaviour
    {

        public List<GameObject> spawnedAgents = new();

        [Header("Agent Data")]
        public int yourTeam = 1;
        [field: SerializeField] public List<GameObject> teamList { get; private set; }

        #region Events
        private void OnEnable()
        {
            GridTile.OnTileClicked += OnTileClicked;
        }

        private void OnDisable()
        {
            GridTile.OnTileClicked -= OnTileClicked;

        }

        #endregion

        private IAgent selectedAgent;
        private void OnTileClicked(IGridTile tileClicked)
        {
            // INFO: Get occupant
            IAgent occupant = tileClicked.isOccupied ? tileClicked.occupiedBy.GetComponent<IAgent>() : null;

            // GUARD: Prevent same selection
            if (selectedAgent == occupant)
                return;


            // INFO: Try to select an agent
            if (tileClicked.isOccupied && (selectedAgent == null || selectedAgent.team == yourTeam))
                selectedAgent = occupant;

            Debug.Log($"{occupant.team} {selectedAgent.team}");

            // Guard: must have a selected agent to move/attack
            if (selectedAgent == null)
                return;

            // INFO: Move to tile
            if (!tileClicked.isOccupied)
            {
                if (selectedAgent.Move(tileClicked.gameObject))
                    selectedAgent = null;

                return;

            }

            // INFO: Attack enemy
            if (occupant.team != selectedAgent.team)
            {
                if (selectedAgent.Attack(tileClicked.gameObject))
                    selectedAgent = null;

                return;
            }


            selectedAgent.CalculateMovement();


        }

        public void SpawnTeam(List<Vector2Int> spawnPositions, int team = 0)
        {
            if (teamList.Count <= 0)
            {
                Debug.LogWarning($"No agents provided!");
                return;

            }
            if (spawnPositions.Count <= 0 || spawnPositions.Count < teamList.Count)
            {
                Debug.LogError($"Not enough spawn positions provided!");
                return;

            }

            // List<GameObject> _agents = new();
            for (int i = 0; i < teamList.Count; i++)
            {
                if (i > spawnPositions.Count) return;

                // INFO: Check valid spawn position before spawning
                GameObject tileGO = GridGenerator.Singleton.GetTileAtOffset(spawnPositions[i].x, spawnPositions[i].y);
                if (tileGO == null) { Debug.LogError($"Failed to get tile at: {spawnPositions[i]}"); return; }
                Vector3 tilePosition = tileGO.transform.position;

                GameObject agentInstance = Instantiate(teamList[i], new Vector3(transform.position.x - 2f + i * 2f, transform.position.y, transform.position.z), Quaternion.identity, transform);
                agentInstance.transform.position = new Vector3(tilePosition.x, 1, tilePosition.z);

                IAgent agent = agentInstance.GetComponent<IAgent>();
                agentInstance.name = agent.agentData.agentName;

                if (agent == null) continue;

                // agent.agentData = teamList[i];
                spawnedAgents.Add(agentInstance);

            }

        }

        #region Utility
        // public List<IAgent> GetAllAgents() => _agents;

        #endregion

    }
}
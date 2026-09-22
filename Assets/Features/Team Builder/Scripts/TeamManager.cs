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

        private List<IAgent> _agents = new();

        [Header("Agent Data")]
        [SerializeField] private List<AgentDataSO> _agentDataList;

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

        private void Start()
        {
            GridGenerator _gridGenerator = GridGenerator.Singleton;
            _gridGenerator.CreateGrid(60, 40, GridLayout.CellLayout.Hexagon, new Vector3(.02f, .02f));

            SpawnTeam();
            Vector3 gridTilePosition = _gridGenerator.GetTileAtOffset(10, 2).transform.position;
            _agents[0].gameObject.transform.position = new Vector3(gridTilePosition.x, 1, gridTilePosition.z);

            SpawnTeam();
            gridTilePosition = _gridGenerator.GetTileAtOffset(10, 1).transform.position;
            _agents[1].gameObject.transform.position = new Vector3(gridTilePosition.x, 1, gridTilePosition.z);
            _agents[1].gameObject.GetComponent<IAgent>().team = 1;

            SpawnTeam();
            gridTilePosition = _gridGenerator.GetTileAtOffset(9, 1).transform.position;
            _agents[2].gameObject.transform.position = new Vector3(gridTilePosition.x, 1, gridTilePosition.z);
            // _agents[2].gameObject.GetComponent<IAgent>().team = 2;

        }

        private IAgent selectedAgent;
        private void OnTileClicked(IGridTile tileClicked)
        {
            // INFO: Get occupant
            IAgent occupant = tileClicked.isOccupied ? tileClicked.occupiedBy.GetComponent<IAgent>() : null;

            // GUARD: Prevent same selection
            if (selectedAgent == occupant)
                return;

            // INFO: Try to select an agent
            if (tileClicked.isOccupied && (selectedAgent == null || occupant.team == selectedAgent.team))
                selectedAgent = occupant;

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

        public void SpawnTeam()
        {
            if (_agentDataList.Count <= 0)
            {
                Debug.LogWarning($"No agents provided!");
                return;

            }

            for (int i = 0; i < _agentDataList.Count; i++)
            {
                GameObject agentInstance = Instantiate(_agentDataList[i].agentPrefab, new Vector3(transform.position.x - 2f + i * 2f, transform.position.y, transform.position.z), Quaternion.identity, transform);
                agentInstance.name = _agentDataList[i].agentName;

                IAgent agent = agentInstance.GetComponent<IAgent>();

                if (agent == null) continue;
                agent.agentData = _agentDataList[i];
                _agents.Add(agent);

            }

        }

        #region Utility
        public List<IAgent> GetAllAgents() => _agents;

        #endregion

    }
}
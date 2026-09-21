using System.Collections.Generic;
using ArenaPrototype.Feature.GridSystem;
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


        private void Start()
        {
            GridGenerator _gridGenerator = GridGenerator.Singleton;
            _gridGenerator.CreateGrid(60, 40, GridLayout.CellLayout.Hexagon, new Vector3(.02f, .02f));

            SpawnTeam();
            Vector3 gridTilePosition = _gridGenerator.GetTileAtOffset(10, 2).transform.position;
            _agents[0].gameObject.transform.position = new Vector3(gridTilePosition.x, 1, gridTilePosition.z);

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

        public List<IAgent> GetAllAgents() => _agents;

    }
}
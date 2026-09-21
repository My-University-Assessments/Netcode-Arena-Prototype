using System;
using UnityEngine;

namespace TeamBuilder.Agents.Data
{
    [CreateAssetMenu(menuName = "Team Builder/New Agent", fileName = "New Agent Data")]
    public class AgentDataSO : ScriptableObject
    {

        [field: Header("Agent Info")]
        [field: SerializeField] public string agentName { get; private set; }
        [field: SerializeField] public MovementType movementType { get; private set; }

        [field: Header("Stats")]
        [field: SerializeField] public int movementRadius { get; private set; } = 1;
        [field: SerializeField] public float attackSpeed { get; private set; }

        [field: Header("Prefab")]
        [field: SerializeField] public GameObject agentPrefab { get; private set; }

    }

    public enum MovementType
    {
        AOE,
        Teleport,
        Other,

    }
}
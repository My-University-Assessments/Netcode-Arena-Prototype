using System.Collections.Generic;
using System.Runtime.InteropServices;
using ArenaPrototype.Feature.GridSystem.Interface;
using TeamBuilder.Agents.Data;
using UnityEngine;


namespace TeamBuilder.Agents.Interface
{
    public interface IAgent
    {
        int team { get; set; }
        AgentDataSO agentData { get; set; }
        GameObject gameObject { get; }

        void CalculateMovement();
        bool Move(GameObject location);
        bool Attack(GameObject target);

    }
}
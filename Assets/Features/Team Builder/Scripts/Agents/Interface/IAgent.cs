using System.Runtime.InteropServices;
using TeamBuilder.Agents.Data;
using UnityEngine;


namespace TeamBuilder.Agents.Interface
{
    public interface IAgent
    {
        AgentDataSO agentData { get; set; }
        GameObject gameObject { get; }

        void CalculateMovement();

    }
}
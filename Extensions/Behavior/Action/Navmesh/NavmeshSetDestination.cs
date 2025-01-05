using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
using UnityEngine.AI;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Set the destination of NavmeshAgent")]
    [CeresLabel("Navmesh: SetDestination")]
    [NodeGroup("Navmesh")]
    public class NavmeshSetDestination : Action
    {
        [SerializeField, Tooltip("If not filled in, it will be obtained from the bound gameObject")]
        private SharedUObject<NavMeshAgent> agent;
        
        [SerializeField]
        private SharedVector3 destination;
        
        protected override Status OnUpdate()
        {
            if (agent != null)
            {
                agent.Value.destination = destination.Value;
            }
            return Status.Success;
        }
        
        public override void Awake()
        {
            if (agent.Value == null) agent.Value = GameObject.GetComponent<NavMeshAgent>();
        }

    }
}

using Ceres;
using Ceres.Annotations;
using UnityEngine;
using UnityEngine.AI;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action : Stop NavmeshAgent according to isStopped")]
    [NodeLabel("Navmesh : StopAgent")]
    [NodeGroup("Navmesh")]
    public class NavmeshStopAgent : Action
    {
        [SerializeField, Tooltip("If not filled in, it will be obtained from the bound gameObject")]
        private SharedUObject<NavMeshAgent> agent;
        [SerializeField, NodeLabel("Whether to stop")]
        private SharedBool isStopped;
        protected override Status OnUpdate()
        {
            if (agent.Value != null && agent.Value.isStopped != isStopped.Value)
            {
                agent.Value.isStopped = isStopped.Value;
            }
            return Status.Success;
        }
        public override void Awake()
        {
            if (agent.Value == null) agent.Value = GameObject.GetComponent<NavMeshAgent>();
        }

    }
}

using System;
using Ceres.Annotations;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Fixed return value," +
    " returns a fixed value after running, you can put the node at the end of the combination logic to keep the return value.")]
    [CeresLabel("Logic: Fixed Return")]
    [CeresGroup("Logic")]
    public class FixedReturn : Action
    {
        public Status returnStatus;
        
        protected override Status OnUpdate()
        {
            return returnStatus;
        }
    }
}
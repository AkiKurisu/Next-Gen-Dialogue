using System.Collections.Generic;
using Ceres;
using Ceres.Annotations;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set random string value")]
    [NodeLabel("String: Random")]
    [NodeGroup("String")]
    public class StringRandom : Action
    {
        public List<string> randomStrings;
        [ForceShared]
        public SharedString storeResult;
        protected override Status OnUpdate()
        {
            storeResult.Value = randomStrings[UnityEngine.Random.Range(0, randomStrings.Count)];
            return Status.Success;
        }
    }
}

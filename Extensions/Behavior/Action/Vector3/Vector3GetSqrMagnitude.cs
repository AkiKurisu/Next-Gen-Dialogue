using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action : Calculate the square of the Vector3 modulus, " +
    "the performance is better than Distance, but the accuracy will be lost")]
    [CeresLabel("Vector3 : GetSqrMagnitude")]
    [NodeGroup("Vector3")]
    public class Vector3GetSqrMagnitude : Action
    {
        [Tooltip("Value to be calculated")]
        public SharedVector3 vector3;
        [ForceShared]
        public SharedFloat result;
        protected override Status OnUpdate()
        {
            result.Value = vector3.Value.sqrMagnitude;
            return Status.Success;
        }
    }
}

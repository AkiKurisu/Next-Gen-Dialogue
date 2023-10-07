using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action : Calculate the square of the Vector3 modulus, " +
    "the performance is better than Distance, but the accuracy will be lost")]
    [AkiLabel("Vector3 : GetSqrMagnitude")]
    [AkiGroup("Vector3")]
    public class Vector3GetSqrMagnitude : Action
    {
        [SerializeField, Tooltip("Value to be calculated")]
        public SharedVector3 vector3;
        [SerializeField, ForceShared]
        private SharedFloat result;
        public override void Awake()
        {
            InitVariable(vector3);
            InitVariable(result);
        }
        protected override Status OnUpdate()
        {
            result.Value = vector3.Value.sqrMagnitude;
            return Status.Success;
        }
    }
}

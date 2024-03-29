using UnityEngine;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Int type set random value")]
    [AkiLabel("Math: IntRandom")]
    [AkiGroup("Math")]
    public class IntRandom : Action
    {
        private enum Operation
        {
            Absolutely,
            Relatively
        }
        [SerializeField]
        private Vector2Int range = new(-5, 5);
        [SerializeField]
        private Operation operation;
        [SerializeField, ForceShared, FormerlySerializedAs("randomInt")]
        private SharedInt storeResult;
        protected override Status OnUpdate()
        {
            int random = UnityEngine.Random.Range(range.x, range.y);
            storeResult.Value = (operation == Operation.Absolutely ? 0 : storeResult.Value) + random;
            return Status.Success;
        }
    }
}
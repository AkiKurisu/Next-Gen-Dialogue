using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Operate bool value")]
    [CeresLabel("Math: BoolOperator")]
    [NodeGroup("Math")]
    public class BoolOperator : Action
    {
        private enum Operation
        {
            And,
            Or
        }
        [SerializeField]
        private SharedBool bool1;
        [SerializeField]
        private SharedBool bool2;
        [SerializeField, ForceShared]
        private SharedBool storeResult;
        [SerializeField]
        private Operation operation;
        protected override Status OnUpdate()
        {
            switch (operation)
            {
                case Operation.And:
                    storeResult.Value = bool1.Value && bool2.Value;
                    break;
                case Operation.Or:
                    storeResult.Value = bool1.Value || bool2.Value;
                    break;
            }
            return Status.Success;
        }
    }
}
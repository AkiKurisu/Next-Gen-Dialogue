using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Condition : Compare Bool values, if the conditions are met, return Status.Success, otherwise return Status.Failure")]
    [AkiLabel("Math : BoolComparison")]
    [AkiGroup("Math")]
    public class BoolComparison : Conditional
    {
        private enum Operation
        {
            EqualTo,
            NotEqualTo,
        }
        [SerializeField]
        private SharedBool bool1;
        [SerializeField]
        private SharedBool bool2;
        [SerializeField]
        private Operation operation;
        protected override void OnStart()
        {
            InitVariable(bool1);
            InitVariable(bool2);
        }
        protected override Status IsUpdatable()
        {
            return operation switch
            {
                Operation.EqualTo => bool1.Value == bool2.Value ? Status.Success : Status.Failure,
                Operation.NotEqualTo => bool1.Value != bool2.Value ? Status.Success : Status.Failure,
                _ => Status.Success,
            };
        }
    }
}
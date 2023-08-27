using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action : Set bool value")]
    [AkiLabel("Math:SetBool")]
    [AkiGroup("Math")]
    public class SetBool : Action
    {
        [SerializeField]
        private bool setValue;
        [SerializeField, ForceShared]
        private SharedBool boolToSet;
        public override void Awake()
        {
            InitVariable(boolToSet);
        }
        protected override Status OnUpdate()
        {
            boolToSet.Value = setValue;
            return Status.Success;
        }
    }
}
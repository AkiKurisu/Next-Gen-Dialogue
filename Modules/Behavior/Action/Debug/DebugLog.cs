using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action : Log some text")]
    [AkiLabel("Debug : Log")]
    [AkiGroup("Debug")]
    public class DebugLog : Action
    {
        [SerializeField]
        private SharedString logText;
        public override void Awake()
        {
            InitVariable(logText);
        }
        protected override Status OnUpdate()
        {
            Debug.Log(logText.Value, GameObject);
            return Status.Success;
        }
    }
}

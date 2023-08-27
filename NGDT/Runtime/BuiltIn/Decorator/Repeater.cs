using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Decorator : Execute the child node repeatedly by the specified number of times" +
    ", if the execution returns Failure, the loop ends and returns Failure")]
    [AkiLabel("Repeater")]
    public class Repeater : Decorator
    {
        [SerializeField]
        private SharedInt repeatCount;
        protected override void OnAwake()
        {
            InitVariable(repeatCount);
        }
        protected override Status OnUpdate()
        {
            for (int i = 0; i < repeatCount.Value; i++)
            {
                var status = Child.Update();
                if (status == Status.Success) continue;
                return status;
            }
            return Status.Success;
        }
    }
}
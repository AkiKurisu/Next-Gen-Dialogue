using Ceres;
using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [NodeInfo("Decorator: Execute the child node repeatedly by the specified number of times" +
    ", if the execution returns Failure, the loop ends and returns Failure")]
    [NodeLabel("Repeater")]
    public class Repeater : Decorator
    {
        public Ceres.SharedInt repeatCount;
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
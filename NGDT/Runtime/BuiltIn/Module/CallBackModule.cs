using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [NodeInfo("Module: CallBack Module is used to add callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class CallBackModule : BehaviorModule
    {
        protected sealed override Status OnUpdate()
        {
            if (Child != null)
            {
                Graph.Builder.GetNode().AddModule(new NGDS.CallBackModule(RunChild));
            }
            return Status.Success;
        }
        private void RunChild()
        {
            Child?.Update();
        }
    }
}

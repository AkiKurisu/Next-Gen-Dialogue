namespace Kurisu.NGDT
{
    [AkiInfo("Module : CallBack Module is used to add callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class CallBackModule : BehaviorModule
    {
        protected sealed override Status OnUpdate()
        {
            if (Child != null)
            {
                Tree.Builder.GetNode().AddModule(new NGDS.CallBackModule(RunChild));
            }
            return Status.Success;
        }
        private void RunChild()
        {
            Child?.Update();
        }
    }
}

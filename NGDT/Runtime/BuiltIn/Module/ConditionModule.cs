namespace Kurisu.NGDT
{
    [AkiInfo("Module : Condition Module is used to add condition for dialogue piece or option,"
    + "if condition fail, parent piece or option will not be added.")]
    [ModuleOf(typeof(Option))]
    [ModuleOf(typeof(Piece))]
    public class ConditionModule : BehaviorModule
    {
        protected sealed override Status OnUpdate()
        {
            if (Child == null) return Status.Success;
            return Child.Update();
        }
    }
}

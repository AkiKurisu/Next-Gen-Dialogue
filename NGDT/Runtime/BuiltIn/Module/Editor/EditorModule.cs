namespace Kurisu.NGDT
{
    /// <summary>
    /// Module only can be used in editor
    /// </summary>
    public abstract class EditorModule : Module
    {
        protected sealed override Status OnUpdate()
        {
            return Status.Success;
        }
    }
}

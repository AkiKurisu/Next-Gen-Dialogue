using System.Collections;
namespace Kurisu.NGDS
{
    public interface IDialogueModule
    {

    }
    /// <summary>
    /// Do things directly after module added
    /// </summary>
    public interface IApplyable
    {
        void Apply(Node parentNode);
    }
    /// <summary>
    /// Do things after inject dependency
    /// </summary>
    public interface IProcessable
    {
        IEnumerator Process(IObjectResolver resolver);
    }
    /// <summary>
    /// Object depdency resolver
    /// </summary>
    public interface IObjectResolver
    {
        T Resolve<T>();
    }
    public interface IContent
    {
        string Content { get; set; }
    }
}

using System.Collections;
namespace Kurisu.NGDS
{
    /// <summary>
    /// Base class for modules in dialogue system
    /// </summary>
    public interface IDialogueModule
    {

    }
    /// <summary>
    /// Apply data directly after module added
    /// </summary>
    public interface IApplyable
    {
        void Apply(Node parentNode);
    }
    /// <summary>
    /// Process data after inject dependency
    /// </summary>
    public interface IProcessable
    {
        IEnumerator Process(IObjectResolver resolver);
    }
    /// <summary>
    /// Object dependency resolver
    /// </summary>
    public interface IObjectResolver
    {
        T Resolve<T>();
    }
    /// <summary>
    /// Interface for changing string content
    /// </summary>
    public interface IContent
    {
        string Content { get; set; }
    }
}

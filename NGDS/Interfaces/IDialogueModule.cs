using System.Collections;
namespace Kurisu.NGDS
{
    public interface IDialogueModule
    {

    }
    public interface IApplyable
    {
        void Apply(DialogueNode parentNode);
    }
    public interface IInjectable
    {
        IEnumerator Inject(IObjectResolver resolver);
    }

    public interface IObjectResolver
    {
        T Resolve<T>();
    }
    public interface IContent
    {
        string Content { get; set; }
    }
}

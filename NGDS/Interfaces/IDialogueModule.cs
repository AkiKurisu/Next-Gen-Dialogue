using System.Threading.Tasks;
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
        Task Inject(IObjectResolver resolver);
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

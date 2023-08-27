using System.Threading.Tasks;
namespace Kurisu.NGDS
{
    public class BuiltInDialogueResolver : IDialogueResolver
    {
        public Dialogue Dialogue { get; private set; }
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(Dialogue dialogue, IDialogueSystem system)
        {
            Dialogue = dialogue;
        }

        public async Task OnDialogueEnter()
        {
            for (int i = 0; i < Dialogue.Modules.Count; i++)
            {
                if (Dialogue.Modules[i] is IInjectable injectable)
                    await injectable.Inject(ObjectContainer);
            }
        }
        public void OnDialogueExit() { }
    }
}

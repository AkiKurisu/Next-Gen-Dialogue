using System.Collections;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS
{
    public readonly struct PromptModule : IDialogueModule, IInjectable
    {
        private readonly string prompt;
        public string Prompt => prompt;
        public PromptModule(string prompt)
        {
            this.prompt = prompt;
        }
        public IEnumerator Inject(IObjectResolver resolver)
        {
            resolver.Resolve<AIPromptBuilder>().SetPrompt(Prompt);
            yield return null;
        }
    }
}

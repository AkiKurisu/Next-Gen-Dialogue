using System.Collections;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS
{
    public readonly struct PromptModule : IDialogueModule, IProcessable
    {
        private readonly string prompt;
        public string Prompt => prompt;
        public PromptModule(string prompt)
        {
            this.prompt = prompt;
        }
        public IEnumerator Process(IObjectResolver resolver)
        {
            resolver.Resolve<AIPromptBuilder>().SetPrompt(Prompt);
            yield return null;
        }
    }
}

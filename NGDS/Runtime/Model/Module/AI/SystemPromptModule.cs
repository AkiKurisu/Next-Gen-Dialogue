using System.Collections;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS
{
    public readonly struct SystemPromptModule : IDialogueModule, IProcessable
    {
        private readonly string prompt;
        public string Prompt => prompt;
        public SystemPromptModule(string prompt)
        {
            this.prompt = prompt;
        }
        public IEnumerator Process(IObjectResolver resolver)
        {
            resolver.Resolve<AIPromptBuilder>().SetSystemPrompt(Prompt);
            yield return null;
        }
    }
}

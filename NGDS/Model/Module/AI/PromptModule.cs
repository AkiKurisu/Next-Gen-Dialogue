using System.Threading.Tasks;
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
        public Task Inject(IObjectResolver resolver)
        {
            resolver.Resolve<AIPromptBuilder>().SetPrompt(Prompt);
            return Task.CompletedTask;
        }
    }
}

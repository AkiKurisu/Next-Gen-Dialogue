using Cysharp.Threading.Tasks;
using NextGenDialogue.AI;
using UnityEngine.Assertions;
namespace NextGenDialogue
{
    public readonly struct SystemPromptModule : IDialogueModule, IProcessable
    {
        public string Prompt { get; }
        
        public SystemPromptModule(string prompt)
        {
            Prompt = prompt;
        }
        
        public UniTask Process(IObjectResolver resolver)
        {
            var builder = resolver.Resolve<AIPromptBuilder>();
            Assert.IsNotNull(builder);
            builder.Context = Prompt;
            return UniTask.CompletedTask;
        }
    }
}

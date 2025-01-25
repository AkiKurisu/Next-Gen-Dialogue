using Cysharp.Threading.Tasks;
using Kurisu.NGDS.AI;
using UnityEngine.Assertions;
namespace Kurisu.NGDS
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

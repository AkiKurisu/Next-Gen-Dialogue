using System.Collections;
using Kurisu.NGDS.AI;
using UnityEngine.Assertions;
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
            var builder = resolver.Resolve<AIPromptBuilder>();
            Assert.IsNotNull(builder);
            builder.Context = Prompt;
            yield break;
        }
    }
}

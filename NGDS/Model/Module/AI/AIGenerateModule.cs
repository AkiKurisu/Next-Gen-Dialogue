using System.Collections;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDS
{
    public readonly struct AIGenerateModule : IDialogueModule, IInjectable
    {
        private readonly string characterName;
        public string CharacterName => characterName;
        public AIGenerateModule(string characterName)
        {
            this.characterName = characterName;
        }

        public IEnumerator Inject(IObjectResolver resolver)
        {
            var promptBuilder = resolver.Resolve<AIPromptBuilder>();
            var task = promptBuilder.Generate(CharacterName);
            yield return new WaitUntil(() => task.IsCompleted);
            var response = task.Result;
            if (response.Status) resolver.Resolve<IContent>().Content = response.Response;
            promptBuilder.Append(CharacterName, response.Response);
        }
    }
}

using System.Collections;
using System.Threading;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDS
{
    public readonly struct AIGenerateModule : IDialogueModule, IProcessable
    {
        private readonly CancellationTokenSource ct;
        private readonly string characterName;
        public string CharacterName => characterName;
        public AIGenerateModule(string characterName)
        {
            this.characterName = characterName;
            ct = new();
        }

        public IEnumerator Process(IObjectResolver resolver)
        {
            var promptBuilder = resolver.Resolve<AIPromptBuilder>();
            var task = promptBuilder.Generate(CharacterName, ct.Token);
            float waitTime = 0f;
            while (!task.IsCompleted)
            {
                yield return null;
                waitTime += Time.deltaTime;
                if (waitTime >= 30f)
                {
                    ct.Cancel();
                    break;
                }
            }
            var response = task.Result;
            if (response.Status) resolver.Resolve<IContent>().Content = response.Response;
            promptBuilder.Append(CharacterName, response.Response);
        }
    }
}

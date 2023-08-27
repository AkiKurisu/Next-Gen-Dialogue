using System.Collections.Generic;
using System.Threading.Tasks;
namespace Kurisu.NGDS.AI
{
    internal struct LLMInput : ILLMInput
    {
        public string Character { get; internal set; }

        public IEnumerable<string> OtherCharacters { get; internal set; }

        public Queue<DialogueParam> History { get; internal set; }
    }
    public struct DialogueParam
    {
        public string Character { get; internal set; }
        public string Content { get; internal set; }
        public DialogueParam(string character, string content)
        {
            Character = character;
            Content = content;
        }
    }
    public class AIPromptBuilder
    {
        private readonly ILLMDriver driver;
        private readonly HashSet<string> characters = new();
        private readonly Queue<DialogueParam> history = new();
        public IReadOnlyCollection<DialogueParam> History => history;
        public string Prompt { get; private set; } = string.Empty;
        public AIPromptBuilder(ILLMDriver driver)
        {
            this.driver = driver;
        }
        public void SetPrompt(string prompt)
        {
            Prompt = prompt;
            driver.SetPrompt(prompt);
        }
        public void Append(string character, string content)
        {
            if (!characters.Contains(character)) characters.Add(character);
            history.Enqueue(new DialogueParam(character, content));
        }
        public async Task<ILLMData> Generate(string character)
        {
            if (characters.Contains(character)) characters.Remove(character);
            var response = await driver.ProcessLLM(new LLMInput()
            {
                Character = character,
                OtherCharacters = characters,
                History = history
            });
#if UNITY_EDITOR
            if (response.Status)
                UnityEngine.Debug.Log("[Detect LLM Response] : " + response.Response);
#endif
            return response;
        }
    }
}

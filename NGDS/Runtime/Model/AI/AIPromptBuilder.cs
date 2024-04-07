using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS.AI
{
    public class DialogueParam : IMessage
    {
        public string character;
        public string content;
        public string Character => character;

        public string Content => content;
        public DialogueParam() { }
        public DialogueParam(string character, string content)
        {
            this.character = character;
            this.content = content;
        }
    }
    public class AIPromptBuilder : ILLMInput
    {
        private readonly ILLMDriver driver;
        public string OutputCharacter { get; private set; }
        private readonly HashSet<string> characters = new();
        public IEnumerable<string> InputCharacters => characters;
        private readonly Queue<DialogueParam> history = new();
        public IEnumerable<IMessage> History => history;
        public string Prompt { get; private set; } = string.Empty;
        public AIPromptBuilder(ILLMDriver driver)
        {
            this.driver = driver;
        }
        public IEnumerable<string> GetCharacters() => characters;
        /// <summary>
        /// Set input system prompt
        /// </summary>
        /// <param name="prompt"></param>
        public void SetSystemPrompt(string prompt)
        {
            Prompt = prompt;
            driver.SetSystemPrompt(prompt);
        }
        /// <summary>
        /// Set input system prompt
        /// </summary>
        /// <param name="prompt"></param>
        public void Append(string character, string content)
        {
            if (!characters.Contains(character)) characters.Add(character);
            history.Enqueue(new DialogueParam(character, content));
        }
        public async Task<ILLMOutput> Generate(string character, CancellationToken ct)
        {
            if (characters.Contains(character)) characters.Remove(character);
            var response = await driver.ProcessLLM(this, ct);
#if UNITY_EDITOR
            if (response.Status)
                UnityEngine.Debug.Log("[Detect LLM Response] " + response.Response);
#endif
            return response;
        }
    }
}

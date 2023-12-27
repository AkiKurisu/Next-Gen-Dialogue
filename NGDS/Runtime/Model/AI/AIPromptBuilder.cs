using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS.AI
{
    internal struct LLMInput : ILLMInput
    {
        public string Character { get; internal set; }

        public IEnumerable<string> OtherCharacters { get; internal set; }

        public Queue<DialogueParam> History { get; internal set; }
    }
    public readonly struct DialogueParam
    {
        public readonly bool HasCharacter;
        public readonly string Character;
        public readonly string Content;
        public DialogueParam(string character, string content)
        {
            HasCharacter = true;
            Character = character;
            Content = content;
        }
        public DialogueParam(string content)
        {
            HasCharacter = false;
            Character = null;
            Content = content;
        }
        public override readonly string ToString()
        {
            if (HasCharacter) return $"{Character}:{Content}";
            else return Content;
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
        public IEnumerable<string> GetCharacters() => characters;
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
        public async Task<ILLMData> Generate(string character, CancellationToken ct)
        {
            if (characters.Contains(character)) characters.Remove(character);
            var response = await driver.ProcessLLM(new LLMInput()
            {
                Character = character,
                OtherCharacters = characters,
                History = history
            }, ct);
#if UNITY_EDITOR
            if (response.Status)
                UnityEngine.Debug.Log("[Detect LLM Response] " + response.Response);
#endif
            return response;
        }
    }
}

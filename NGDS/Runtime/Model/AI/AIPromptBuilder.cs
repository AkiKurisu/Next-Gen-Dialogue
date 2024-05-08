using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS.AI
{
    public class DialogueParam : IMessage
    {
        public string Content { get; private set; }
        public MessageRole Role { get; private set; }
        public DialogueParam() { }
        public DialogueParam(MessageRole role, string content)
        {
            Role = role;
            Content = content;
        }
    }
    public class AIPromptBuilder : ILLMRequest
    {
        private readonly ILargeLanguageModel llm;
        private readonly Queue<DialogueParam> history = new();
        public IEnumerable<IMessage> History => history;
        public string Context { get; private set; } = string.Empty;
        public AIPromptBuilder(ILargeLanguageModel driver)
        {
            llm = driver;
        }
        /// <summary>
        /// Set input system prompt
        /// </summary>
        /// <param name="prompt"></param>
        public void SetSystemPrompt(string prompt)
        {
            Context = prompt;
        }
        /// <summary>
        /// Set input system prompt
        /// </summary>
        /// <param name="prompt"></param>
        public void Append(MessageRole role, string content)
        {
            history.Enqueue(new DialogueParam(role, content));
        }
        public async Task<ILLMResponse> GenerateAsync(CancellationToken ct)
        {
            return await llm.GenerateAsync(this, ct);
        }
    }
}

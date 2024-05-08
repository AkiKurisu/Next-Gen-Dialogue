using System.Collections.Generic;
namespace Kurisu.NGDS.AI
{
    public class AIPromptBuilder : ILLMRequest
    {
        public class DialogueParam : IMessage
        {
            public string Content { get; private set; }
            public MessageRole Role { get; private set; }
            public DialogueParam(MessageRole role, string content)
            {
                Role = role;
                Content = content;
            }
        }
        private readonly Queue<DialogueParam> history = new();
        public IEnumerable<IMessage> Messages => history;
        public string Context { get; set; } = string.Empty;
        /// <summary>
        /// Set input system prompt
        /// </summary>
        /// <param name="prompt"></param>
        public void Append(MessageRole role, string content)
        {
            history.Enqueue(new DialogueParam(role, content));
        }
    }
}

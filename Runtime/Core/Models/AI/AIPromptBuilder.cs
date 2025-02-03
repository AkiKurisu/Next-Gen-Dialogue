using System.Collections.Generic;
using System.Text;
namespace NextGenDialogue.AI
{
    public class AIPromptBuilder : ILLMRequest
    {
        private class DialogueParam : IMessage
        {
            public string Content { get; set; }
            public MessageRole Role { get; set; }
            public DialogueParam(MessageRole role, string content)
            {
                Role = role;
                Content = content;
            }
        }
        private readonly Queue<DialogueParam> history = new();
        public IEnumerable<IMessage> Messages => history;
        public string UserName { get; set; } = "User";
        public string BotName { get; set; } = "Bot";
        public string Context { get; set; } = string.Empty;
        public void Append(MessageRole role, string content)
        {
            history.Enqueue(new DialogueParam(role, content));
        }
        public void ReverseRole()
        {
            foreach (var message in history)
            {
                if (message.Role == MessageRole.User) message.Role = MessageRole.Bot;
                else if (message.Role == MessageRole.Bot) message.Role = MessageRole.User;
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var message in history)
            {
                sb.AppendLine($"{(message.Role == MessageRole.User ? UserName : BotName)}: {message.Content}");
            }
            return sb.ToString();
        }
    }
}

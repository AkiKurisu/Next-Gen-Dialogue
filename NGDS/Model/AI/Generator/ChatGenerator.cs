using System.Text;
namespace Kurisu.NGDS.AI
{
    public class ChatGenerator
    {
        private readonly StringBuilder stringBuilder = new();
        public string Generate(ILLMInput llmInput)
        {
            stringBuilder.Clear();
            while (llmInput.History.TryDequeue(out DialogueParam param))
            {
                stringBuilder.Append(param.Character);
                stringBuilder.Append(':');
                stringBuilder.Append(param.Content);
                stringBuilder.Append('\n');
            }
            stringBuilder.Append(llmInput.Character);
            stringBuilder.Append(':');
            stringBuilder.Append('\n');
            return stringBuilder.ToString();
        }
    }
}

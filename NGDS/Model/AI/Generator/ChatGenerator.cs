using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        public async Task Generate(ILLMInput llmInput, List<SendData> sendDatas, GoogleTranslateModule? preTranslateModule)
        {
            while (llmInput.History.TryDequeue(out DialogueParam param))
            {
                string content = param.Content;
                if (preTranslateModule.HasValue)
                {
                    content = await preTranslateModule.Value.Process(content);
                }
                var sendData = new SendData(param.Character == llmInput.Character ? "assistant" : "user", content);
                sendDatas.Add(sendData);
            }

        }
    }
}

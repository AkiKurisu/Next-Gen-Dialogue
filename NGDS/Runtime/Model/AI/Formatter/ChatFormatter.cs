using System.Collections.Generic;
using System.Text;
namespace Kurisu.NGDS.AI
{
    /// <summary>
    /// Format a chat-style input
    /// </summary>
    public class ChatFormatter
    {
        private readonly StringBuilder stringBuilder = new();
        /// <summary>
        /// Format input and return concat string
        /// </summary>
        /// <param name="llmInput"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public string Format(ILLMInput llmInput)
        {
            stringBuilder.Clear();
            foreach (var param in llmInput.History)
            {
                stringBuilder.AppendLine(param.ToString());
            }
            stringBuilder.Append(llmInput.OutputCharacter);
            stringBuilder.Append(':');
            stringBuilder.Append('\n');
            return stringBuilder.ToString();
        }
        /// <summary>
        /// Format input and add to openai-style <see cref="SendData"\> list
        /// </summary>
        /// <param name="llmInput"></param>
        /// <param name="sendDataList"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public void Format(ILLMInput llmInput, List<SendData> sendDataList)
        {
            foreach (var param in llmInput.History)
            {
                string content = param.content;
                var sendData = new SendData(param.character == llmInput.OutputCharacter ? "assistant" : "user", content);
                sendDataList.Add(sendData);
            }
        }
    }
}

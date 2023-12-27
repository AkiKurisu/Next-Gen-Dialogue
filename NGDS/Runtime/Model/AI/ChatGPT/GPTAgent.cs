using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Kurisu.NGDS.AI
{
    /// <summary>
    /// Pure ChatGPT agent for any task
    /// </summary>
    public class GPTAgent
    {
        private readonly List<SendData> m_DataList = new();
        private const string m_gptModel = "gpt-3.5-turbo";
        public string SystemPrompt { get; set; } = string.Empty;
        private readonly ILLMDriver driver;
        public GPTAgent(ILLMDriver driver)
        {
            this.driver = driver;
        }
        public async Task<string> Inference(string inputPrompt, CancellationToken ct)
        {
            m_DataList.Clear();
            m_DataList.Add(new SendData("system", SystemPrompt));
            m_DataList.Add(new SendData("user", inputPrompt));
            PostData _postData = new()
            {
                model = m_gptModel,
                messages = m_DataList
            };
            string input = JsonUtility.ToJson(_postData);
#if UNITY_EDITOR
            Debug.Log(input);
#endif
            var response = await driver.ProcessLLM(input, ct);
            return response.Response;
        }
    }
}
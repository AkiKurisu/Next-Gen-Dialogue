using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Kurisu.NGDS.AI
{
    /// <summary>
    /// Pure GPT agent for any task followed openAI api format, working with ChatGLM2,ChatGLM3 and ChatGPT
    /// </summary>
    public class GPTAgent
    {
        private readonly List<SendData> m_DataList = new();
        public const string DefaultModel = "gpt-3.5-turbo";
        /// <summary>
        /// The model agent used
        /// </summary>
        /// <value></value>
        public string Model { get; set; } = null;
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
                model = Model ?? DefaultModel,
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
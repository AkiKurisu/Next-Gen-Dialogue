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
        private readonly SemaphoreSlim semaphore = new(1, 1);
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
        /// <summary>
        /// Call GPT without history
        /// </summary>
        /// <param name="inputPrompt"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string> Inference(string inputPrompt, CancellationToken ct)
        {
            await semaphore.WaitAsync();
            try
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
                if (response.Status)
                {
                    m_DataList.Add(new SendData("assistant", response.Response));
                }
                return response.Response;
            }
            finally
            {
                semaphore.Release();
            }
        }
        /// <summary>
        /// Call GPT with history
        /// </summary>
        /// <param name="inputPrompt"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string> Continue(string inputPrompt, CancellationToken ct)
        {
            await semaphore.WaitAsync();
            try
            {
                if (m_DataList.Count == 0) m_DataList.Add(new SendData("system", SystemPrompt));
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
                if (response.Status)
                {
                    m_DataList.Add(new SendData("assistant", response.Response));
                }
                return response.Response;
            }
            finally
            {
                semaphore.Release();
            }
        }
        /// <summary>
        /// Clear history context
        /// </summary>
        public void ClearHistory()
        {
            m_DataList.Clear();
        }
        /// <summary>
        /// Append history context
        /// </summary>
        /// <param name="histories"></param>
        public void Append(IEnumerable<(string userData, string assistantData)> histories)
        {
            foreach (var (userData, assistantData) in histories)
            {
                Append(userData, assistantData);
            }
        }
        public void Append(string userData, string assistantData)
        {
            if (!string.IsNullOrEmpty(userData))
                m_DataList.Add(new SendData("user", userData));
            if (!string.IsNullOrEmpty(userData))
                m_DataList.Add(new SendData("assistant", assistantData));
        }
    }
}
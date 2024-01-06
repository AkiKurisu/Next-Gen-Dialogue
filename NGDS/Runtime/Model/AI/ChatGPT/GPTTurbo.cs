using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.NGDS.AI
{
    internal class GPTTurbo : ILLMDriver
    {
        private struct GPTResponse : ILLMData
        {
            public bool Status { get; internal set; }
            public string Response { get; internal set; }
        }
        private const string DefaultAPI = "https://api.openai-proxy.com/v1/chat/completions";
        private string ChatAPI { get; set; }
        private readonly string m_gptModel;
        private readonly List<SendData> m_DataList = new();
        private readonly SendData promptData;
        private readonly string openAIKey;
        private bool promptIsProcessed;
        private readonly ChatGenerator chatGenerator = new();
        public GoogleTranslateModule? PreTranslateModule { get; set; }
        private readonly bool chatMode;
        public GPTTurbo(string url, string m_gptModel, string openAIKey, bool chatMode)
        {
            promptData = new SendData("system", string.Empty);
            m_DataList.Add(promptData);
            this.openAIKey = openAIKey;
            this.chatMode = chatMode;
            this.m_gptModel = m_gptModel;
            if (string.IsNullOrEmpty(url))
                ChatAPI = DefaultAPI;
            else
                ChatAPI = url;
        }
        public void SetPrompt(string prompt)
        {
            promptData.content = prompt;
        }
        public async Task<ILLMData> ProcessLLM(ILLMInput input, CancellationToken ct)
        {
            await AppendContent(input, ct);
            PostData _postData = new()
            {
                model = m_gptModel,
                messages = m_DataList
            };
            return await ProcessLLM(JsonUtility.ToJson(_postData), ct);
        }
        public async Task<ILLMData> ProcessLLM(string input, CancellationToken ct)
        {
            using UnityWebRequest request = new(ChatAPI, "POST");
            byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", openAIKey));
            request.SendWebRequest();
            while (!request.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
            if (request.responseCode == 200)
            {
                string _msg = request.downloadHandler.text;
                MessageBack messageBack = JsonUtility.FromJson<MessageBack>(_msg);
                string _backMsg = string.Empty;
                if (messageBack != null && messageBack.choices.Count > 0)
                {
                    _backMsg = messageBack.choices[0].message.content;
                }
                return new GPTResponse()
                {
                    Response = _backMsg,
                    Status = true
                };
            }
            NGDSLogger.LogError($"ChatGPT ResponseCode: {request.responseCode}\nResponse: {request.downloadHandler.text}");
            return new GPTResponse()
            {
                Response = string.Empty,
                Status = false
            };
        }
        private async Task AppendContent(ILLMInput input, CancellationToken ct)
        {
            m_DataList.Clear();
            m_DataList.Add(promptData);
            if (chatMode)
            {
                await chatGenerator.Generate(input, m_DataList, PreTranslateModule, ct);
            }
            else
            {
                string generatedPrompt = chatGenerator.Generate(input);
                if (PreTranslateModule.HasValue)
                {
                    generatedPrompt = await PreTranslateModule.Value.Process(generatedPrompt, ct);
                }
                m_DataList.Add(new SendData("user", generatedPrompt));
            }
            if (PreTranslateModule.HasValue && !promptIsProcessed)
            {
                promptIsProcessed = true;
                promptData.content = await PreTranslateModule.Value.Process(promptData.content, ct); ;
            }
        }
    }
}


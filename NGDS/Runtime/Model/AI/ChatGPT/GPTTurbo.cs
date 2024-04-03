using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.NGDS.AI
{
    internal class GPTTurbo : ILLMDriver
    {
        private struct GPTResponse : ILLMOutput
        {
            public bool Status { get; internal set; }
            public string Response { get; internal set; }
        }
        private const string DefaultAPI = "https://api.openai-proxy.com/v1/chat/completions";
        private string ChatAPI { get; set; }
        public string GptModel { get; set; }
        private readonly List<SendData> m_DataList = new();
        private readonly SendData promptData;
        public string ApiKey { get; set; }
        private bool promptIsProcessed;
        private readonly ChatFormatter formatter = new();
        public ITranslator Translator { get; set; }
        public bool ChatMode { get; set; }
        public GPTTurbo(string url, string model, string apiKey, bool chatMode)
        {
            promptData = new SendData("system", string.Empty);
            m_DataList.Add(promptData);
            ApiKey = apiKey;
            ChatMode = chatMode;
            GptModel = model;
            if (string.IsNullOrEmpty(url))
                ChatAPI = DefaultAPI;
            else
                ChatAPI = url;
        }
        public void SetSystemPrompt(string prompt)
        {
            promptData.content = prompt;
        }
        public async Task<ILLMOutput> ProcessLLM(ILLMInput input, CancellationToken ct)
        {
            await AppendContent(input, ct);
            PostData _postData = new()
            {
                model = GptModel,
                messages = m_DataList
            };
            return await ProcessLLM(JsonUtility.ToJson(_postData), ct);
        }
        public async Task<ILLMOutput> ProcessLLM(string input, CancellationToken ct)
        {
            using UnityWebRequest request = new(ChatAPI, "POST");
            byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", ApiKey));
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
            if (ChatMode)
            {
                formatter.Format(input, m_DataList);
            }
            else
            {
                string generatedPrompt = formatter.Format(input);
                m_DataList.Add(new SendData("user", generatedPrompt));
            }
            if (Translator != null && !promptIsProcessed)
            {
                promptIsProcessed = true;
                promptData.content = await Translator.Translate(promptData.content, ct); ;
            }
        }
    }
}


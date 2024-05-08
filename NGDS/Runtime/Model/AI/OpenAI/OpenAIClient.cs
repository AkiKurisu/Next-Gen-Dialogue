using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.NGDS.AI
{
    public class OpenAIClient : ILargeLanguageModel
    {
        private struct GPTResponse : ILLMResponse
        {
            public string Response { get; internal set; }
        }
        public float Temperature { get; set; } = 0.5f;
        public float Top_p { get; set; } = 0.5f;
        public const string DefaultModel = "gpt-3.5-turbo";
        private const string DefaultAPI = "https://api.openai-proxy.com/v1/chat/completions";
        private string Api { get; set; }
        public string Model { get; set; } = DefaultModel;
        private readonly List<SendData> m_DataList = new();
        private readonly SendData promptData;
        public string ApiKey { get; set; }
        private bool promptIsProcessed;
        public ITranslator Translator { get; set; }
        public OpenAIClient(string url, string model, string apiKey)
        {
            promptData = new SendData("system", string.Empty);
            m_DataList.Add(promptData);
            ApiKey = apiKey;
            Model = model;
            if (string.IsNullOrEmpty(url))
                Api = DefaultAPI;
            else
                Api = url;
        }
        public async Task<ILLMResponse> GenerateAsync(ILLMRequest input, CancellationToken ct)
        {
            promptData.content = input.Context; ;
            await AppendContent(input, ct);
            PostData _postData = new()
            {
                model = Model,
                messages = m_DataList
            };
            return await InternalCall(JsonUtility.ToJson(_postData), ct);
        }
        public async Task<ILLMResponse> GenerateAsync(string input, CancellationToken ct)
        {
            m_DataList.Clear();
            m_DataList.Add(promptData);
            m_DataList.Add(new SendData("user", input));
            PostData _postData = new()
            {
                model = Model,
                messages = m_DataList
            };
            return await InternalCall(JsonUtility.ToJson(_postData), ct);
        }
        public async Task<ILLMResponse> InternalCall(string message, CancellationToken ct)
        {
            using UnityWebRequest request = new(Api, "POST");
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
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
            if (request.responseCode != 200) throw new Exception(request.error);
            string _msg = request.downloadHandler.text;
            MessageBack messageBack = JsonUtility.FromJson<MessageBack>(_msg);
            string _backMsg = string.Empty;
            if (messageBack != null && messageBack.choices.Count > 0)
            {
                _backMsg = messageBack.choices[0].message.content;
            }
            return new GPTResponse()
            {
                Response = _backMsg
            };
        }
        private async Task AppendContent(ILLMRequest request, CancellationToken ct)
        {
            m_DataList.Clear();
            m_DataList.Add(promptData);
            Format(request, m_DataList);
            if (Translator != null && !promptIsProcessed)
            {
                promptIsProcessed = true;
                promptData.content = await Translator.Translate(promptData.content, ct); ;
            }
        }
        private static void Format(ILLMRequest request, List<SendData> sendDataList)
        {
            foreach (var param in request.History)
            {
                string content = param.Content;
                var sendData = new SendData(param.Role == MessageRole.Bot ? "assistant" : "user", content);
                sendDataList.Add(sendData);
            }
        }
    }
}


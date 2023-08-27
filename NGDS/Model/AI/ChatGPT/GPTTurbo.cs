using System.Collections.Generic;
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
        private const string chatAPI = "https://api.openai-proxy.com/v1/chat/completions";
        private const string m_gptModel = "gpt-3.5-turbo";
        private readonly List<SendData> m_DataList = new();
        private readonly SendData promptData;
        private readonly string openAIKey;
        private bool promptIsProcessed;
        private readonly ChatGenerator chatGenerator = new();
        public GoogleTranslateModule? PreTranslateModule { get; set; }
        public GPTTurbo(string openAIKey)
        {
            promptData = new SendData("system", string.Empty);
            m_DataList.Add(promptData);
            this.openAIKey = openAIKey;
        }
        public void SetPrompt(string prompt)
        {
            promptData.content = prompt;
        }
        public async Task<ILLMData> ProcessLLM(ILLMInput input)
        {
            string generatedPrompt = chatGenerator.Generate(input);
            if (PreTranslateModule.HasValue)
            {
                generatedPrompt = await PreTranslateModule.Value.Process(generatedPrompt);
                if (!promptIsProcessed)
                {
                    promptIsProcessed = true;
                    promptData.content = await PreTranslateModule.Value.Process(promptData.content); ;
                }
            }
            var lastSend = new SendData("user", generatedPrompt);
            m_DataList.Add(lastSend);
            using UnityWebRequest request = new(chatAPI, "POST");
            PostData _postData = new()
            {
                model = m_gptModel,
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", openAIKey));
            request.SendWebRequest();
            while (!request.isDone)
            {
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
                    m_DataList.Add(new SendData("assistant", _backMsg));
                }
                return new GPTResponse()
                {
                    Response = _backMsg,
                    Status = true
                };
            }
            NGDSLogger.LogError($"ChatGPT ResponseCode : {request.responseCode}\nResponse : {request.downloadHandler.text}");
            m_DataList.Remove(lastSend);
            return new GPTResponse()
            {
                Response = string.Empty,
                Status = false
            };
        }
    }
}


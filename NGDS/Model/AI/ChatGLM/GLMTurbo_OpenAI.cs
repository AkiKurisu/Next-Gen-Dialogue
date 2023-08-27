using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.NGDS.AI
{
    /// <summary>
    /// Use ChatGLM with OpenAI API format to generate chat text
    /// See https://github.com/THUDM/ChatGLM2-6B/blob/main/openai_api.py
    /// </summary>
    internal class GLMTurbo_OpenAI : ILLMDriver
    {
        private struct GLMResponse : ILLMData
        {
            public bool Status { get; internal set; }

            public string Response { get; internal set; }
        }
        private string _baseUri;
        private const string m_gptModel = "gpt-3.5-turbo";
        private readonly List<SendData> m_DataList = new();
        private readonly SendData promptData;
        private bool promptIsProcessed;
        private readonly ChatGenerator chatGenerator = new();
        public GoogleTranslateModule? PreTranslateModule { get; set; }
        public GLMTurbo_OpenAI(string address = "127.0.0.1", string port = "8000")
        {
            _baseUri = $"http://{address}:{port}/v1/chat/completions";
            promptData = new SendData("system", string.Empty);
            m_DataList.Add(promptData);
        }
        public void SetPrompt(string prompt)
        {
            promptData.content = prompt;
            promptIsProcessed = false;
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
            using UnityWebRequest request = new(_baseUri, "POST");
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
                return new GLMResponse()
                {
                    Response = _backMsg,
                    Status = true
                };
            }
            m_DataList.Remove(lastSend);
            return new GLMResponse()
            {
                Response = string.Empty,
                Status = false
            };
        }
    }
}


using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;
namespace Kurisu.NGDS.AI
{
    /// <summary>
    /// Use ChatGLM with Normal API to generate text
    /// See https://github.com/THUDM/ChatGLM2-6B/blob/main/api.py
    /// </summary>
    internal class GLMTurbo : ILLMDriver
    {
        private struct GLMResponse : ILLMData
        {
            public bool Status { get; internal set; }

            public string Response { get; internal set; }
        }
        private readonly string _baseUri;
        private string _basePrompt;
        private readonly GLMGenParams genParams;
        private readonly ChatGenerator chatGenerator = new();
        public GoogleTranslateModule? PreTranslateModule { get; set; }
        public GLMTurbo(string address = "127.0.0.1", string port = "8000")
        {
            _baseUri = $"http://{address}:{port}/";
            genParams = new();
        }
        public void SetPrompt(string newPrompt)
        {
            _basePrompt = newPrompt;
        }
        public async Task<ILLMData> ProcessLLM(ILLMInput input)
        {
            string generatedPrompt = chatGenerator.Generate(input);
            if (!string.IsNullOrEmpty(_basePrompt))
            {
                genParams.Prompt = $"{_basePrompt}\n{generatedPrompt}";
                _basePrompt = null;
            }
            else
            {
                genParams.Prompt = generatedPrompt;
            }
            if (PreTranslateModule.HasValue)
            {
                genParams.Prompt = await PreTranslateModule.Value.Process(genParams.Prompt);
            }
            UnityWebRequest request = new(_baseUri, "POST")
            {
                uploadHandler =
                     new UploadHandlerRaw(new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(genParams))),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            request.SendWebRequest();
            while (!request.isDone)
            {
                await Task.Yield();
            }
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                NGDSLogger.LogError(request.error);
                return new GLMResponse()
                {
                    Status = false,
                    Response = string.Empty
                };
            }
            else
            {
                string response = string.Empty;
                bool validate;
                try
                {
                    var messageBack = JsonConvert.DeserializeObject<GLMMessageBack>(request.downloadHandler.text);
                    response = messageBack.Response;
                    genParams.History = messageBack.History;
                    validate = true;
                }
                catch (Exception e)
                {
                    NGDSLogger.LogError(e.Message);
                    validate = false;
                }
                return new GLMResponse()
                {
                    Response = response,
                    Status = validate
                };
            }
        }
    }
}

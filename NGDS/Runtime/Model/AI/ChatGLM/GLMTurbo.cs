using System;
using System.Text;
using System.Threading;
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
        private struct GLMResponse : ILLMOutput
        {
            public bool Status { get; internal set; }

            public string Response { get; internal set; }
        }
        private readonly string _baseUri;
        private string _basePrompt;
        private readonly GLMGenParams genParams;
        private readonly ChatFormatter formatter = new();
        public GLMTurbo(string address = "127.0.0.1", string port = "8000")
        {
            _baseUri = $"http://{address}:{port}/";
            genParams = new();
        }
        public void SetSystemPrompt(string newPrompt)
        {
            _basePrompt = newPrompt;
        }
        public async Task<ILLMOutput> ProcessLLM(ILLMInput input, CancellationToken ct)
        {
            string generatedPrompt = formatter.Format(input);
            if (!string.IsNullOrEmpty(_basePrompt))
            {
                genParams.Prompt = $"{_basePrompt}\n{generatedPrompt}";
                _basePrompt = null;
            }
            else
            {
                genParams.Prompt = generatedPrompt;
            }
            return await ProcessLLM(JsonConvert.SerializeObject(genParams), ct);
        }
        public async Task<ILLMOutput> ProcessLLM(string input, CancellationToken ct)
        {
            UnityWebRequest request = new(_baseUri, "POST")
            {
                uploadHandler =
                     new UploadHandlerRaw(new UTF8Encoding().GetBytes(input)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            request.SendWebRequest();
            while (!request.isDone)
            {
                ct.ThrowIfCancellationRequested();
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

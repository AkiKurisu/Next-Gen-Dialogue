using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;
namespace Kurisu.NGDS.AI
{
    public class OobaboogaTurbo : ILLMDriver
    {
        private struct OobaboogaResponse : ILLMOutput
        {
            public bool Status { get; internal set; }

            public string Response { get; internal set; }
        }
        private string prompt;
        private readonly StringBuilder stringBuilder = new();
        private readonly ChatFormatter formatter = new();
        public ITranslator Translator { get; set; }
        private readonly OobaboogaGenerateParams genParams = new();
        private readonly string _baseUri;
        public static string[] replaceKeyWords = new string[]
        {
            "<START>"
        };
        public OobaboogaTurbo(string address = "127.0.0.1", string port = "5000")
        {
            _baseUri = $"http://{address}:{port}";
        }
        private void SetStopCharacter(IEnumerable<string> stopCharacters)
        {
            genParams.StopStrings = new();
            foreach (var char_name in stopCharacters)
            {
                genParams.StopStrings.Add(char_name);
                genParams.StopStrings.Add($"\n{char_name} ");
            }
        }
        public void SetSystemPrompt(string prompt)
        {
            this.prompt = prompt;
        }
        public async Task<ILLMOutput> ProcessLLM(ILLMInput input, CancellationToken ct)
        {
            SetStopCharacter(input.InputCharacters);
            string message = formatter.Format(input);
            if (Translator != null) message = await Translator.Translate(message, ct);
            return await SendMessageToOobaboogaAsync(message, ct);
        }
        public async Task<ILLMOutput> ProcessLLM(string input, CancellationToken ct)
        {
            return await SendMessageToOobaboogaAsync(input, ct);
        }
        private async Task<OobaboogaResponse> SendMessageToOobaboogaAsync(string message, CancellationToken ct)
        {
            string response = string.Empty;
            stringBuilder.Clear();
            stringBuilder.Append(prompt);
            stringBuilder.Append(message);
            genParams.Prompt = stringBuilder.ToString();
            try
            {
                //TODO: Add chat mode
                using UnityWebRequest request = new($"{_baseUri}/api/v1/generate", "POST");
                byte[] data = Encoding.UTF8.GetBytes(genParams.ToJson());
                request.uploadHandler = new UploadHandlerRaw(data);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SendWebRequest();
                while (!request.isDone)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
                if (request.responseCode == 200)
                {
                    var result = JsonConvert.DeserializeObject<ModelOutput>(request.downloadHandler.text.Trim());
                    response = result.Results[0].Text;
                    return new OobaboogaResponse()
                    {
                        Status = true,
                        Response = FormatResponse(response)
                    };
                }
                NGDSLogger.LogError($"Oobabooga ResponseCode: {request.responseCode}\nResponse: {request.downloadHandler.text}");
                return new OobaboogaResponse()
                {
                    Response = string.Empty,
                    Status = false
                };
            }
            catch (Exception e)
            {
                NGDSLogger.LogError(e.Message);
                return new OobaboogaResponse()
                {
                    Status = false,
                    Response = response
                };
            }
        }
        private string FormatResponse(string response)
        {
            if (string.IsNullOrEmpty(response)) return string.Empty;
            response = LineBreakHelper.Format(response);
            foreach (var keyword in replaceKeyWords)
            {
                response = response.Replace(keyword, string.Empty);
            }
            foreach (var stopWord in genParams.StopStrings)
            {
                response = response.Replace(stopWord, string.Empty);
            }
            return response;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS.AI
{
    public class OobaboogaTurbo : ILLMDriver
    {
        private struct OobaboogaResponse : ILLMData
        {
            public bool Status { get; internal set; }

            public string Response { get; internal set; }
        }
        private readonly OobaboogaClient client;
        private readonly OobaboogaParams genParams;
        private readonly ChatGenerator chatGenerator = new();
        public GoogleTranslateModule? PreTranslateModule { get; set; }

        private static readonly string[] replaceKeyWords = new string[]
        {
            "<START>","END_OF_DIALOGUE","END_OF_ACTIVE_ANSWER"
        };
        public OobaboogaTurbo(string address = "127.0.0.1", string port = "5000")
        {
            client = new OobaboogaClient($"http://{address}:{port}",
                        genParams = new()
                    );
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
        public void SetPrompt(string prompt)
        {
            client.SetPrompt(prompt);
        }
        public async Task<ILLMData> ProcessLLM(ILLMInput input, CancellationToken ct)
        {
            SetStopCharacter(input.OtherCharacters);
            string message = chatGenerator.Generate(input);
            if (PreTranslateModule.HasValue)
            {
                message = await PreTranslateModule.Value.Process(message, ct);
            }
            return await SendMessageToOobaboogaAsync(message, ct);
        }
        public async Task<ILLMData> ProcessLLM(string input, CancellationToken ct)
        {
            return await SendMessageToOobaboogaAsync(input, ct);
        }
        private async Task<OobaboogaResponse> SendMessageToOobaboogaAsync(string message, CancellationToken ct)
        {
            string response = string.Empty;
            try
            {
                var result = await client.Generate(message, ct);
                response = result.Results[0].Text;
                return new OobaboogaResponse()
                {
                    Status = true,
                    Response = FormatResponse(response)
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

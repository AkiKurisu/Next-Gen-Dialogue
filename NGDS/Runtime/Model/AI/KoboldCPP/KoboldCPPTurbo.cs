using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS.AI
{
    public class CharacterPresetHelper
    {
        public static string GeneratePrompt(string user_Name = "You", string char_name = "Bot", string char_persona = null, string world_scenario = null)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Clear();
            //This Prompt is the same as KoboldAI generated, useful for generating chat text
            stringBuilder.Append($"[The following is an interesting chat message log between {user_Name} and {char_name}.");
            //This Prompt is added by me and may be modified in future, you can have a test and change it to your version.
            //Note: 
            //Those Prompts are designed to generate a chat text. 
            //If you want other forms like story mode, prompt should be changed. 
            if (!string.IsNullOrEmpty(char_persona))
                stringBuilder.Append($"\n{char_name}'s persona : {char_persona}");
            if (!string.IsNullOrEmpty(world_scenario))
                stringBuilder.Append($"\nWorld's scenario : {world_scenario}");
            //<START> means chat beginning
            stringBuilder.Append("]\n<START>\n");
            return stringBuilder.ToString();
        }
    }
    public class KoboldCPPTurbo : ILLMDriver
    {
        private struct KoboldResponse : ILLMData
        {
            public bool Status { get; internal set; }

            public string Response { get; internal set; }
        }
        private readonly KoboldClient client;
        private readonly KoboldGenParams genParams;
        private readonly ChatGenerator chatGenerator = new();
        public GoogleTranslateModule? PreTranslateModule { get; set; }

        private static readonly string[] replaceKeyWords = new string[]
        {
            "<START>","END_OF_DIALOGUE","END_OF_ACTIVE_ANSWER"
        };
        public KoboldCPPTurbo(string address = "127.0.0.1", string port = "5001")
        {
            client = new KoboldClient($"http://{address}:{port}",
                        genParams = new()
                    );
        }
        private void SetStopCharacter(IEnumerable<string> stopCharacters)
        {
            genParams.StopSequence = new();
            foreach (var char_name in stopCharacters)
            {
                genParams.StopSequence.Add(char_name);
                genParams.StopSequence.Add($"\n{char_name} ");
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
            return await SendMessageToKoboldAsync(message, ct);
        }
        public async Task<ILLMData> ProcessLLM(string input, CancellationToken ct)
        {
            return await SendMessageToKoboldAsync(input, ct);
        }
        private async Task<KoboldResponse> SendMessageToKoboldAsync(string message, CancellationToken ct)
        {
            string response = string.Empty;
            try
            {
                var result = await client.Generate(message, ct);
                response = result.Results[0].Text;
                return new KoboldResponse()
                {
                    Status = true,
                    Response = FormatResponse(response)
                };
            }
            catch (Exception e)
            {
                NGDSLogger.LogError(e.Message);
                return new KoboldResponse()
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
            foreach (var stopWord in genParams.StopSequence)
            {
                response = response.Replace(stopWord, string.Empty);
            }
            return response;
        }
    }
    public class LineBreakHelper
    {
        public static string Format(string input)
        {
            int startIndex = 0;
            int endIndex = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != '\n')
                {
                    startIndex = i;
                    break;
                }
            }
            for (int i = input.Length - 1; i >= 0; i--)
            {
                if (input[i] != '\n')
                {
                    endIndex = i;
                    break;
                }
            }
            if (startIndex > endIndex) return string.Empty;
            return input.Substring(startIndex, endIndex - startIndex + 1);
        }
    }
}

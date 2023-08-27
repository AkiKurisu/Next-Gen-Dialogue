using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Kurisu.NGDS.AI
{
    //Modify from https://github.com/pboardman/KoboldSharp
    internal class KoboldClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUri;
        private string prompt;
        private readonly KoboldGenParams genParams;
        private readonly StringBuilder stringBuilder = new();
        public KoboldClient(string baseUri, KoboldGenParams genParams)
        {
            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _baseUri = baseUri;
            this.genParams = genParams;
        }
        public void SetPrompt(string newPrompt)
        {
            prompt = newPrompt;
        }
        public async Task<ModelOutput> Generate(string message)
        {
            stringBuilder.Clear();
            stringBuilder.Append(prompt);
            stringBuilder.Append(message);
            genParams.Prompt = stringBuilder.ToString();
            var payload = new StringContent(genParams.GetJson(), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_baseUri}/api/v1/generate", payload);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            content = content.Trim();
            return JsonConvert.DeserializeObject<ModelOutput>(content);
        }
    }
}

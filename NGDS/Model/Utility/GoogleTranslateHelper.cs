using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;
namespace Kurisu.NGDS
{
    public struct GoogleTranslateResponse
    {
        public bool Status { get; internal set; }
        public string SourceText { get; internal set; }
        public string TranslateText { get; internal set; }
    }
    public class GoogleTranslateHelper
    {
        private const string DefaultSL = "auto";
        public static async Task<GoogleTranslateResponse> TranslateTextAsync(string sourceLanguage, string targetLanguage, string input)
        {
            StringBuilder stringBuilder = new();
            string url;
            if (string.IsNullOrEmpty(sourceLanguage)) sourceLanguage = DefaultSL;
            url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={UnityWebRequest.EscapeURL(input)}";
            var webRequest = UnityWebRequest.Get(url);
            webRequest.SendWebRequest();
            while (!webRequest.isDone)
            {
                await Task.Yield();
            }
            if (webRequest.result is UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError(webRequest.error);
                return new GoogleTranslateResponse()
                {
                    Status = false,
                    SourceText = input,
                    TranslateText = string.Empty
                };
            }
            JToken parsedTexts = JToken.Parse(webRequest.downloadHandler.text);
            if (parsedTexts != null && parsedTexts[0] != null)
            {
                var jsonArray = parsedTexts[0].AsJEnumerable();

                if (jsonArray != null)
                {
                    foreach (JToken innerArray in jsonArray)
                    {
                        JToken text = innerArray[0];

                        if (text != null)
                        {
                            stringBuilder.Append(text);
                            stringBuilder.Append(' ');
                        }
                    }
                }
            }
            return new GoogleTranslateResponse()
            {
                Status = true,
                SourceText = input,
                TranslateText = stringBuilder.ToString().Trim()
            };
        }
    }
}

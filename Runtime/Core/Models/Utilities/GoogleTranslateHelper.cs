using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue
{
    public struct GoogleTranslateResponse
    {
        public bool Status { get; internal set; }
        
        public string SourceText { get; internal set; }
        
        public string TranslateText { get; internal set; }
    }
    
    public class GoogleTranslateHelper
    {
        private const string DefaultSourceLanguage = "auto";
        
        public static async UniTask<GoogleTranslateResponse> TranslateTextAsync(string sourceLanguage, string targetLanguage, string input, CancellationToken ct)
        {
            StringBuilder stringBuilder = new();
            string url;
            if (string.IsNullOrEmpty(sourceLanguage)) sourceLanguage = DefaultSourceLanguage;
            url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={UnityWebRequest.EscapeURL(input)}";
            var webRequest = UnityWebRequest.Get(url);
            await webRequest.SendWebRequest().ToUniTask(cancellationToken: ct);
            if (webRequest.result is UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError(webRequest.error);
                return new GoogleTranslateResponse
                {
                    Status = false,
                    SourceText = input,
                    TranslateText = string.Empty
                };
            }
            try
            {
                JToken parsedTexts = JToken.Parse(webRequest.downloadHandler.text);
                if (parsedTexts[0] != null)
                {
                    var jsonArray = parsedTexts[0].AsJEnumerable();

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
                return new GoogleTranslateResponse
                {
                    Status = true,
                    SourceText = input,
                    TranslateText = stringBuilder.ToString().Trim()
                };
            }
            catch
            {
                Debug.LogError("[Google Translate] Translation Failed!");
                return new GoogleTranslateResponse
                {
                    Status = false,
                    SourceText = input,
                    TranslateText = string.Empty
                };
            }

        }
    }
}

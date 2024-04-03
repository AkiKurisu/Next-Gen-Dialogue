using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Threading;
namespace Kurisu.NGDS.Translator
{
    public class GoogleTranslator : ITranslator
    {
        public string SourceLanguageCode { get; set; }
        public string TargetLanguageCode { get; set; }
        public GoogleTranslator() { }
        public GoogleTranslator(string sourceLanguageCode, string targetLanguageCode)
        {
            SourceLanguageCode = sourceLanguageCode;
            TargetLanguageCode = targetLanguageCode;
        }
        public async Task<string> Translate(string input, CancellationToken ct)
        {
            if (SourceLanguageCode == TargetLanguageCode) return input;
            try
            {
                var response = await GoogleTranslateHelper.TranslateTextAsync(SourceLanguageCode, TargetLanguageCode, input, ct);
                if (response.Status) return response.TranslateText;
                else return input;
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("[Google Translator] Translation is out of time!");
                return input;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return input;
            }
        }
    }
}

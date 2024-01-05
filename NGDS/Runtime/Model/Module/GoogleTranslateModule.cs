using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
namespace Kurisu.NGDS
{
    public readonly struct GoogleTranslateModule : IDialogueModule, IProcessable
    {
        public string SourceLanguageCode { get; }
        public string TargetLanguageCode { get; }
        public GoogleTranslateModule(string sourceLanguageCode, string targetLanguageCode)
        {
            SourceLanguageCode = sourceLanguageCode;
            TargetLanguageCode = targetLanguageCode;
        }
        public GoogleTranslateModule(string targetLanguageCode)
        {
            SourceLanguageCode = null;
            TargetLanguageCode = targetLanguageCode;
        }
        public async Task<string> Process(string input, CancellationToken ct)
        {
            if (SourceLanguageCode == TargetLanguageCode) return input;
            try
            {
                var response = await GoogleTranslateHelper.TranslateTextAsync(SourceLanguageCode, TargetLanguageCode, input, ct);
                if (response.Status) return response.TranslateText;
                else return input;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return input;
            }
        }
        public IEnumerator Process(IObjectResolver resolver)
        {
            var content = resolver.Resolve<IContent>();
            var task = Process(content.Content, new());
            yield return new WaitUntil(() => task.IsCompleted);
            content.Content = task.Result;
        }
    }
}

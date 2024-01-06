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
        private const float MaxWaitTime = 30f;
        private readonly CancellationTokenSource ct;
        public GoogleTranslateModule(string sourceLanguageCode, string targetLanguageCode)
        {
            SourceLanguageCode = sourceLanguageCode;
            TargetLanguageCode = targetLanguageCode;
            ct = new();
        }
        public GoogleTranslateModule(string targetLanguageCode)
        {
            SourceLanguageCode = null;
            TargetLanguageCode = targetLanguageCode;
            ct = new();
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
            catch (OperationCanceledException)
            {
                Debug.LogError("[Google Translate] Translation is out of time!");
                return input;
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
            var task = Process(content.Content, ct.Token);
            float waitTime = 0;
            while (!task.IsCompleted)
            {
                yield return null;
                waitTime += Time.deltaTime;
                if (waitTime >= MaxWaitTime)
                {
                    ct.Cancel();
                    break;
                }
            }
            content.Content = task.Result;
        }
    }
}

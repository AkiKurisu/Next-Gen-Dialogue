using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue.Translator
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
        
        public async UniTask<string> TranslateAsync(string input, CancellationToken ct)
        {
            if (SourceLanguageCode == TargetLanguageCode) return input;
            try
            {
                var response = await GoogleTranslateHelper.TranslateTextAsync(SourceLanguageCode, TargetLanguageCode, input, ct);
                if (response.Status) return response.TranslateText;
                return input;
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
        
        public async UniTask TranslateAsyncBatch(List<string> inputs, CancellationToken ct)
        {
            if (SourceLanguageCode == TargetLanguageCode) return;
            try
            {
                var responses = await UniTask.WhenAll(inputs.Select(x => GoogleTranslateHelper.TranslateTextAsync(SourceLanguageCode, TargetLanguageCode, x, ct)));
                inputs.Clear();
                foreach (var response in responses)
                {
                    if (response.Status) inputs.Add(response.TranslateText);
                    else inputs.Add(response.SourceText);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("[Google Translator] Translation is out of time!");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }
}

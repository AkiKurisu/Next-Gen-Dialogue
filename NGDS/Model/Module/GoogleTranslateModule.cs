using UnityEngine;
using System.Threading.Tasks;
using System;
namespace Kurisu.NGDS
{
    public readonly struct GoogleTranslateModule : IDialogueModule, IInjectable
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
        public async Task<string> Process(string input)
        {
            if (SourceLanguageCode == TargetLanguageCode) return input;
            try
            {
                var response = await GoogleTranslateHelper.TranslateTextAsync(SourceLanguageCode, TargetLanguageCode, input);
                if (response.Status) return response.TranslateText;
                else return input;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return input;
            }

        }
        public async Task Inject(IObjectResolver resolver)
        {
            var content = resolver.Resolve<IContent>();
            content.Content = await Process(content.Content);
        }
    }
}

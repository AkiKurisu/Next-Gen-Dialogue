using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NextGenDialogue.Translator;
using UnityEngine.Pool;

namespace NextGenDialogue
{
    public readonly struct GoogleTranslateModule : IDialogueModule, IProcessable
    {
        private readonly GoogleTranslator _googleTranslator;
        
        private const float MaxWaitTime = 30f;
        
        public GoogleTranslateModule(string sourceLanguageCode, string targetLanguageCode)
        {
            _googleTranslator = new GoogleTranslator(sourceLanguageCode, targetLanguageCode);
        }
        
        public GoogleTranslateModule(string targetLanguageCode)
        {
            _googleTranslator = new GoogleTranslator(null, targetLanguageCode);
        }
        
        public async UniTask Process(IObjectResolver resolver, CancellationToken cancellationToken)
        {
            var contentModule = resolver.Resolve<IContentModule>();
            var contents = ListPool<string>.Get();
            {
                contentModule.GetContents(contents);
                await _googleTranslator.TranslateAsyncBatch(contents, cancellationToken)
                    .Timeout(TimeSpan.FromSeconds(MaxWaitTime));
                contentModule.SetContents(contents);
            }
            ListPool<string>.Release(contents);
        }
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kurisu.NGDS.Translator;
using UnityEngine.Pool;
namespace Kurisu.NGDS
{
    public readonly struct GoogleTranslateModule : IDialogueModule, IProcessable
    {
        private readonly GoogleTranslator _googleTranslator;
        
        private const float MaxWaitTime = 30f;
        
        private readonly CancellationTokenSource _cts;
        
        public GoogleTranslateModule(string sourceLanguageCode, string targetLanguageCode)
        {
            _googleTranslator = new GoogleTranslator(sourceLanguageCode, targetLanguageCode);
            _cts = new CancellationTokenSource();
        }
        
        public GoogleTranslateModule(string targetLanguageCode)
        {
            _googleTranslator = new GoogleTranslator(null, targetLanguageCode);
            _cts = new CancellationTokenSource();
        }
        public async UniTask Process(IObjectResolver resolver)
        {
            var contentModule = resolver.Resolve<IContentModule>();
            var contents = ListPool<string>.Get();
            {
                contentModule.GetContents(contents);
                await _googleTranslator.TranslateAsyncBatch(contents, _cts.Token)
                    .Timeout(TimeSpan.FromSeconds(MaxWaitTime));
                contentModule.SetContents(contents);
            }
            ListPool<string>.Release(contents);
        }
    }
}

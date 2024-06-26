using UnityEngine;
using System.Collections;
using System.Threading;
using Kurisu.NGDS.Translator;
namespace Kurisu.NGDS
{
    public readonly struct GoogleTranslateModule : IDialogueModule, IProcessable
    {
        private readonly GoogleTranslator googleTranslator;
        private const float MaxWaitTime = 30f;
        private readonly CancellationTokenSource ct;
        public GoogleTranslateModule(string sourceLanguageCode, string targetLanguageCode)
        {
            googleTranslator = new GoogleTranslator(sourceLanguageCode, targetLanguageCode);
            ct = new();
        }
        public GoogleTranslateModule(string targetLanguageCode)
        {
            googleTranslator = new GoogleTranslator(null, targetLanguageCode);
            ct = new();
        }
        public IEnumerator Process(IObjectResolver resolver)
        {
            var content = resolver.Resolve<IContent>();
            var task = googleTranslator.Translate(content.Content, ct.Token);
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

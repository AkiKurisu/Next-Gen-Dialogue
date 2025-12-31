using System.Threading;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue.Translator
{
    public class LLMTranslator : ITranslator
    {
        private readonly ILargeLanguageModel _llm;
        
        public string Prompt { get; set; }
        
        public LLMTranslator(ILargeLanguageModel llm, string sourceLanguage, string targetLanguage)
        {
            _llm = llm;
            if (sourceLanguage != null)
                Prompt = $"{targetLanguage} and {sourceLanguage} are language codes. You should translate {sourceLanguage} to {targetLanguage}. You should only reply the translation.";
            else
                Prompt = $"{targetLanguage} is language code. You should detect my language and translate them to {targetLanguage}. You should only reply the translation.";
        }
        
        public async UniTask<string> TranslateAsync(string input, CancellationToken ct)
        {
            return (await _llm.GenerateAsync($"{Prompt}\n{input}", ct)).Response;
        }
    }
}
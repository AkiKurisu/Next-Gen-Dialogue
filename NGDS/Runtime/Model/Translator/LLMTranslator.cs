using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS.Translator
{
    public class LLMTranslator : ITranslator
    {
        private readonly ILargeLanguageModel llm;
        public string Prompt { get; set; }
        public LLMTranslator(ILargeLanguageModel llm, string sourceLanguage, string targetLanguage)
        {
            this.llm = llm;
            if (sourceLanguage != null)
                Prompt = $"{targetLanguage} and {sourceLanguage} are language codes. You should translate {sourceLanguage} to {targetLanguage}. You should only reply the translation.";
            else
                Prompt = $"{targetLanguage} is language code. You should detect my language and translate them to {targetLanguage}. You should only reply the translation.";
        }
        public async Task<string> Translate(string input, CancellationToken ct)
        {
            return (await llm.GenerateAsync($"{Prompt}\n{input}", ct)).Response;
        }
    }
}
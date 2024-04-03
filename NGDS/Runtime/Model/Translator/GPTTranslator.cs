using System.Threading;
using System.Threading.Tasks;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS.Translator
{
    public class GPTTranslator : ITranslator
    {
        private readonly GPTAgent agent;
        public GPTTranslator(GPTAgent agent, string sourceLanguage, string targetLanguage)
        {
            this.agent = agent;
            if (string.IsNullOrEmpty(agent.SystemPrompt))
            {
                if (sourceLanguage != null)
                    agent.SystemPrompt = $"{targetLanguage} and {sourceLanguage} are language codes. You should translate {sourceLanguage} to {targetLanguage}. You should only reply the translation.";
                else
                    agent.SystemPrompt = $"{targetLanguage} is language code. You should detect my language and translate them to {targetLanguage}. You should only reply the translation.";
            }
        }
        public async Task<string> Translate(string input, CancellationToken ct)
        {
            return await agent.Inference(input, ct);
        }
    }
}
using Kurisu.NGDS.Translator;
namespace Kurisu.NGDS.AI
{
    public enum LLMType
    {
        OpenAI
    }
    public enum TranslatorType
    {
        None, Google, LLM
    }
    public class LLMFactory
    {
        public static ILargeLanguageModel CreateLLM(AITurboSetting setting)
        {
            ILargeLanguageModel driver = setting.LLM_Type switch
            {
                //GPT models support multilingual input
                LLMType.OpenAI => new OpenAIClient(setting.ChatGPT_URL_Override, setting.GPT_Model, setting.OpenAIKey),
                _ => null,
            };
            return driver;
        }
        public static ITranslator CreateTranslator(TranslatorType translatorType, AITurboSetting setting)
        {
            return translatorType switch
            {
                TranslatorType.Google => new GoogleTranslator(null, setting.LLM_Language),
                TranslatorType.LLM => new LLMTranslator(CreateLLM(setting), null, setting.LLM_Language),
                _ => null,
            };
        }
        public static ITranslator CreateTranslator(TranslatorType translatorType, AITurboSetting setting, string sourceLanguage, string targetLanguage)
        {
            return translatorType switch
            {
                TranslatorType.Google => new GoogleTranslator(sourceLanguage, targetLanguage),
                TranslatorType.LLM => new LLMTranslator(CreateLLM(setting), sourceLanguage, targetLanguage),
                _ => null,
            };
        }
    }
}

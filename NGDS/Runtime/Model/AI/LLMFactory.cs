using Kurisu.NGDS.Translator;
namespace Kurisu.NGDS.AI
{
    public enum LLMType
    {
        ChatGPT, KoboldCPP, Oobabooga, ChatGLM
    }
    public enum TranslatorType
    {
        None, Google, GPT
    }
    public class LLMFactory
    {
        public static ILLMDriver Create(LLMType llmType, AITurboSetting setting)
        {
            ILLMDriver driver = llmType switch
            {
                //GPT models support multilingual input
                LLMType.ChatGPT => new GPTTurbo(setting.ChatGPT_URL_Override, setting.GPT_Model, setting.OpenAIKey, setting.ChatMode),
                LLMType.ChatGLM => new GLMTurbo(setting.LLM_Address, setting.LLM_Port),
                //Add translator for need
                LLMType.KoboldCPP => new KoboldCPPTurbo(setting.LLM_Address, setting.LLM_Port)
                {
                    Translator = CreateTranslator(setting.TranslatorType, setting)
                },
                //Tips: Oobabooga can set google translation in server
                LLMType.Oobabooga => new OobaboogaTurbo(setting.LLM_Address, setting.LLM_Port)
                {
                    Translator = CreateTranslator(setting.TranslatorType, setting)
                },
                _ => null,
            };
            return driver;
        }
        public static ITranslator CreateTranslator(TranslatorType translatorType, AITurboSetting aiTurboSetting)
        {
            return translatorType switch
            {
                TranslatorType.Google => new GoogleTranslator(null, aiTurboSetting.LLM_Language),
                TranslatorType.GPT => new GPTTranslator(new GPTAgent(Create(LLMType.ChatGPT, aiTurboSetting)), null, aiTurboSetting.LLM_Language),
                _ => null,
            };
        }
        public static ITranslator CreateTranslator(TranslatorType translatorType, AITurboSetting aiTurboSetting, string sourceLanguage, string targetLanguage)
        {
            return translatorType switch
            {
                TranslatorType.Google => new GoogleTranslator(sourceLanguage, targetLanguage),
                TranslatorType.GPT => new GPTTranslator(new GPTAgent(Create(LLMType.ChatGPT, aiTurboSetting)), sourceLanguage, targetLanguage),
                _ => null,
            };
        }
    }
}

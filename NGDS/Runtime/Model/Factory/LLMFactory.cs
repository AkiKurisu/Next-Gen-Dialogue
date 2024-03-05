namespace Kurisu.NGDS.AI
{
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
                    Translator = setting.Enable_GoogleTranslation ? new GoogleTranslateModule(setting.LLM_Language) : null
                },
                //Tips: Oobabooga can set google translation in server
                LLMType.Oobabooga => new OobaboogaTurbo(setting.LLM_Address, setting.LLM_Port)
                {
                    Translator = setting.Enable_GoogleTranslation ? new GoogleTranslateModule(setting.LLM_Language) : null
                },
                _ => null,
            };
            return driver;
        }
    }
}

namespace Kurisu.NGDS.AI
{
    public class LLMFactory
    {
        public static ILLMDriver Create(LLMType llmType, AITurboSetting setting)
        {
            var driver = CreateNonModule(llmType, setting);
            if (setting.Enable_GoogleTranslation)
            {
                driver.PreTranslateModule = new(setting.LLM_Language);
            }
            return driver;
        }
        public static ILLMDriver CreateNonModule(LLMType llmType, AITurboSetting setting)
        {
            ILLMDriver driver = llmType switch
            {
                LLMType.ChatGPT => new GPTTurbo(setting.ChatGPT_URL_Override, setting.OpenAIKey, setting.ChatMode),
                LLMType.KoboldCPP => new KoboldCPPTurbo(setting.LLM_Address, setting.LLM_Port),
                LLMType.Oobabooga => new OobaboogaTurbo(setting.LLM_Address, setting.LLM_Port),
                LLMType.ChatGLM => new GLMTurbo(setting.LLM_Address, setting.LLM_Port),
                LLMType.ChatGLM_OpenAI => new GLMTurbo_OpenAI(setting.LLM_Address, setting.LLM_Port, setting.ChatMode),
                _ => null,
            };
            return driver;
        }
    }
}

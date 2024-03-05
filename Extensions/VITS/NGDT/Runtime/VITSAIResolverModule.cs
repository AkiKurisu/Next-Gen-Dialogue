#if NGD_USE_VITS
using Kurisu.NGDS;
using Kurisu.NGDS.AI;
using Kurisu.NGDS.VITS;
using UnityEngine;
namespace Kurisu.NGDT.VITS
{
    [AkiInfo("Module : Specify VITS-AI dialogue resolver for this dialogue tree.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class VITSAIResolverModule : CustomModule
    {
        public LLMType llmType;
        public SharedTObject<AITurboSetting> setting;
        public SharedTObject<AudioSource> audioSource;
        [Setting]
        public bool overrideDialogueResolver = true;
        [Setting]
        public bool overridePieceResolver = true;
        [Setting]
        public bool overrideOptionResolver = true;
        protected override IDialogueModule GetModule()
        {
            var turboSetting = setting.Value;
            var vitsTurbo = new VITSTurbo(turboSetting)
            {
                Translator = turboSetting.Enable_GoogleTranslation ? new NGDS.GoogleTranslateModule(turboSetting.LLM_Language, turboSetting.VITS_Language) : null
            };
            var builder = new AIPromptBuilder(LLMFactory.Create(llmType, turboSetting));
            return new ResolverModule(
                    overrideDialogueResolver ? new AIDialogueResolver(builder) : null,
                    overridePieceResolver ? new VITSPieceResolver(builder, vitsTurbo, audioSource.Value) : null,
                    overrideOptionResolver ? new VITSOptionResolver(builder, vitsTurbo, audioSource.Value) : null
                );
        }
    }
}
#endif
using Kurisu.NGDS;
using Kurisu.NGDS.AI;
using Kurisu.NGDS.VITS;
using UnityEngine;
namespace Kurisu.NGDT.VITS
{
    [AkiInfo("Module : Specify VITS dialogue resolver for this dialogue tree.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class VITSResolverModule : CustomModule
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
                Translator = LLMFactory.CreateTranslator(turboSetting.TranslatorType, turboSetting, turboSetting.LLM_Language, turboSetting.VITS_Language)
            };
            return new ResolverModule(
                    overrideDialogueResolver ? new BuiltInDialogueResolver() : null,
                    overridePieceResolver ? new VITSPieceResolver(vitsTurbo, audioSource.Value) : null,
                    overrideOptionResolver ? new VITSOptionResolver(vitsTurbo, audioSource.Value) : null
                );
        }
    }
}
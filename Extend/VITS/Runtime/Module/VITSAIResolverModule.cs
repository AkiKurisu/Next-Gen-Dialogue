#if USE_VITS
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
        [SerializeField]
        private LLMType llmType;
        [SerializeField]
        private SharedTObject<AITurboSetting> setting;
        [SerializeField]
        private SharedTObject<AudioSource> audioSource;
        [Setting, SerializeField]
        private bool overrideDialogueResolver = true;
        [Setting, SerializeField]
        private bool overridePieceResolver = true;
        [Setting, SerializeField]
        private bool overrideOptionResolver = true;
        public override void Awake()
        {
            InitVariable(setting);
            InitVariable(audioSource);
        }
        protected override IDialogueModule GetModule()
        {
            var turboSetting = setting.Value;
            var vitsTurbo = new VITSTurbo(turboSetting)
            {
                PreTranslateModule = turboSetting.Enable_GoogleTranslation ? new(turboSetting.LLM_Language, turboSetting.VITS_Language) : null
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
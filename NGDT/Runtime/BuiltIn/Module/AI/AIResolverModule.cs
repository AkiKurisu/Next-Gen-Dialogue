using Kurisu.NGDS;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : Specify AI dialogue resolver for this dialogue tree.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class AIResolverModule : CustomModule
    {
        [SerializeField]
        private LLMType llmType;
        [SerializeField]
        private SharedTObject<AITurboSetting> setting;
        [Setting, SerializeField]
        private bool overrideDialogueResolver = true;
        [Setting, SerializeField]
        private bool overridePieceResolver = true;
        [Setting, SerializeField]
        private bool overrideOptionResolver = true;
        public override void Awake()
        {
            InitVariable(setting);
        }
        protected override IDialogueModule GetModule()
        {
            var builder = new AIPromptBuilder(LLMFactory.Create(llmType, setting.Value));
            return new ResolverModule(
                    overrideDialogueResolver ? new AIDialogueResolver(builder) : null,
                    overridePieceResolver ? new AIPieceResolver(builder) : null,
                    overrideOptionResolver ? new AIOptionResolver(builder) : null
                );
        }
    }
}

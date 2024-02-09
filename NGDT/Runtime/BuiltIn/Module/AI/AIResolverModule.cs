using Kurisu.NGDS;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDT
{
    [AkiInfo("Module: Specify AI dialogue resolver for this dialogue tree.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Dialogue))]
    public class AIResolverModule : CustomModule
    {
        public LLMType llmType;
        public SharedTObject<AITurboSetting> setting;
        [Setting]
        public bool overrideDialogueResolver = true;
        [Setting]
        public bool overridePieceResolver = true;
        [Setting]
        public bool overrideOptionResolver = true;
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

using UnityEngine;
namespace Kurisu.NGDS.AI
{
    public class AIEntry : MonoBehaviour
    {
        [SerializeField]
        private LLMType llmType;
        [SerializeField]
        private AITurboSetting setting;
        private IPieceResolver pieceResolver;
        private IOptionResolver optionResolver;
        private IDialogueResolver dialogueResolver;
        private void Awake()
        {
            var builder = new AIPromptBuilder(LLMFactory.Create(llmType, setting));
            IOCContainer.Register(pieceResolver = new AIPieceResolver(builder));
            IOCContainer.Register(optionResolver = new AIOptionResolver(builder));
            IOCContainer.Register(dialogueResolver = new AIDialogueResolver(builder));
        }
        private void OnDestroy()
        {
            IOCContainer.UnRegister(pieceResolver);
            IOCContainer.UnRegister(optionResolver);
            IOCContainer.UnRegister(dialogueResolver);
        }
    }
}

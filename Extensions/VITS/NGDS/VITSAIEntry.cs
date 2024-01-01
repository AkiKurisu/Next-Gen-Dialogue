#if NGD_USE_VITS
using UnityEngine;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS.VITS
{
    public class VITSAIEntry : MonoBehaviour
    {

        [SerializeField]
        private LLMType llmType;
        [SerializeField]
        private AITurboSetting setting;
        [SerializeField]
        private AudioSource audioSource;
        private IPieceResolver pieceResolver;
        private IOptionResolver optionResolver;
        private IDialogueResolver dialogueResolver;
        private VITSTurbo vitsTurbo;
        public AudioClip AudioClipCache => vitsTurbo?.AudioClipCache;
        private void Awake()
        {
            vitsTurbo = new VITSTurbo(setting)
            {
                //Auto detect language, not specify source language
                PreTranslateModule = setting.Enable_GoogleTranslation ? new(setting.VITS_Language) : null
            };
            var builder = new AIPromptBuilder(LLMFactory.Create(llmType, setting));
            IOCContainer.Register(vitsTurbo);
            IOCContainer.Register(pieceResolver = new VITSPieceResolver(builder, vitsTurbo, audioSource));
            IOCContainer.Register(optionResolver = new VITSOptionResolver(builder, vitsTurbo, audioSource));
            IOCContainer.Register(dialogueResolver = new AIDialogueResolver(builder));
        }
        private void OnDestroy()
        {
            IOCContainer.UnRegister(vitsTurbo);
            IOCContainer.UnRegister(pieceResolver);
            IOCContainer.UnRegister(optionResolver);
            IOCContainer.UnRegister(dialogueResolver);
        }
    }

}
#endif
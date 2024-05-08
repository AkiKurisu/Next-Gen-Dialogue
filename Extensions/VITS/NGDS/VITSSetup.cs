using UnityEngine;
using Kurisu.NGDS.AI;
namespace Kurisu.NGDS.VITS
{
    public class VITSSetup : MonoBehaviour
    {
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
                Translator = LLMFactory.CreateTranslator(setting.TranslatorType, setting, setting.LLM_Language, setting.VITS_Language)
            };
            IOCContainer.Register(vitsTurbo);
            IOCContainer.Register(pieceResolver = new VITSPieceResolver(vitsTurbo, audioSource));
            IOCContainer.Register(optionResolver = new VITSOptionResolver(vitsTurbo, audioSource));
            IOCContainer.Register(dialogueResolver = new DefaultDialogueResolver());
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
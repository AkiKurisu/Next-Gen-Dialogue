using Chris.Gameplay;
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
        
        private IPieceResolver _pieceResolver;
        
        private IOptionResolver _optionResolver;
        
        private IDialogueResolver _dialogueResolver;
        
        private VITSTurbo _vitsTurbo;
        
        public AudioClip AudioClipCache => _vitsTurbo?.AudioClipCache;
        
        private void Awake()
        {
            _vitsTurbo = new VITSTurbo(setting)
            {
                // Auto-detect language, not specify source language
                Translator = LLMFactory.CreateTranslator(setting.TranslatorType, setting, setting.LLM_Language, setting.VITS_Language)
            };
            ContainerSubsystem.Get().Register(_vitsTurbo);
            ContainerSubsystem.Get().Register(_pieceResolver = new VITSPieceResolver(_vitsTurbo, audioSource));
            ContainerSubsystem.Get().Register(_optionResolver = new VITSOptionResolver(_vitsTurbo, audioSource));
            ContainerSubsystem.Get().Register(_dialogueResolver = new DefaultDialogueResolver());
        }
        
        private void OnDestroy()
        {
            ContainerSubsystem.Get()?.Unregister(_vitsTurbo);
            ContainerSubsystem.Get()?.Unregister(_pieceResolver);
            ContainerSubsystem.Get()?.Unregister(_optionResolver);
            ContainerSubsystem.Get()?.Unregister(_dialogueResolver);
        }
    }

}
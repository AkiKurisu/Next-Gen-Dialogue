using Kurisu.Framework;
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
                // Auto-detect language, not specify source language
                Translator = LLMFactory.CreateTranslator(setting.TranslatorType, setting, setting.LLM_Language, setting.VITS_Language)
            };
            ContainerSubsystem.Get().Register(vitsTurbo);
            ContainerSubsystem.Get().Register(pieceResolver = new VITSPieceResolver(vitsTurbo, audioSource));
            ContainerSubsystem.Get().Register(optionResolver = new VITSOptionResolver(vitsTurbo, audioSource));
            ContainerSubsystem.Get().Register(dialogueResolver = new DefaultDialogueResolver());
        }
        private void OnDestroy()
        {
            ContainerSubsystem.Get()?.Unregister(vitsTurbo);
            ContainerSubsystem.Get()?.Unregister(pieceResolver);
            ContainerSubsystem.Get()?.Unregister(optionResolver);
            ContainerSubsystem.Get()?.Unregister(dialogueResolver);
        }
    }

}
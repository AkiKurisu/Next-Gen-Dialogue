using UnityEngine;
namespace Kurisu.NGDS.VITS
{
    public readonly struct VITSGenerateModule : IDialogueModule
    {
        public readonly int CharacterID { get; }
        public readonly bool NoTranslation { get; }
        public VITSGenerateModule(int characterID, bool noTranslation)
        {
            CharacterID = characterID;
            NoTranslation = noTranslation;
        }
    }
    public readonly struct VITSAudioClipModule : IDialogueModule
    {
        public AudioClip AudioClip { get; }
        public VITSAudioClipModule(AudioClip audioClip)
        {
            AudioClip = audioClip;
        }
    }
}

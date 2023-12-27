using UnityEngine;
namespace Kurisu.NGDS.VITS
{
    public readonly struct VITSGenerateModule : IDialogueModule
    {
        private readonly int characterID;
        public int CharacterID => characterID;
        public VITSGenerateModule(int characterID)
        {
            this.characterID = characterID;
        }
    }
    public readonly struct VITSAudioClipModule : IDialogueModule
    {
        private readonly AudioClip audioClip;
        public AudioClip AudioClip => audioClip;
        public VITSAudioClipModule(AudioClip audioClip)
        {
            this.audioClip = audioClip;
        }
    }
}

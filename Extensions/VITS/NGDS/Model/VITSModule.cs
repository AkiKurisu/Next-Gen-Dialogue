using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Kurisu.NGDS.VITS
{
    public class VITSModule : IDialogueModule
    {
        public int CharacterID { get; }
        
        public bool NoTranslation { get; }
        
        public AudioClip AudioClip { get; }
        
        public VITSModule(int characterID, bool noTranslation)
        {
            CharacterID = characterID;
            NoTranslation = noTranslation;
            AudioClip = null;
        }
        
        public VITSModule(AudioClip audioClip)
        {
            AudioClip = audioClip;
            CharacterID = 0;
            NoTranslation = false;
        }
        
        public async UniTask RequestOrLoadAudioClipParallel(int i, VITSTurbo vitsTurbo, string[] contents, AudioClip[] results, CancellationToken token)
        {
            if (AudioClip)
            {
                results[i] = AudioClip;
                return;
            }
            VITSResponse response;
            if (NoTranslation)
                response = await vitsTurbo.SendVITSRequestAsync(contents[i], CharacterID, token);
            else
                response = await vitsTurbo.SendVITSRequestAsyncWithTranslation(contents[i], CharacterID, token);
            if (response.Status)
            {
                results[i] = response.Result;
            }
        }
        
        public async UniTask<AudioClip> RequestOrLoadAudioClip(VITSTurbo vitsTurbo, string content, CancellationToken token)
        {
            if (AudioClip)
            {
                return AudioClip;
            }
            VITSResponse response;
            if (NoTranslation)
                response = await vitsTurbo.SendVITSRequestAsync(content, CharacterID, token);
            else
                response = await vitsTurbo.SendVITSRequestAsyncWithTranslation(content, CharacterID, token);
            if (response.Status)
            {
                return response.Result;
            }
            return null;
        }
    }
}

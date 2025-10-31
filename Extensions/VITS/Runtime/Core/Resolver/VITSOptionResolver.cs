using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace NextGenDialogue.VITS
{
    public class VITSOptionResolver : IOptionResolver
    {
        public VITSOptionResolver(VITSTurbo vitsTurbo, AudioSource audioSource)
        {
            _vitsTurbo = vitsTurbo;
            _audioSource = audioSource;
        }
        
        private readonly VITSTurbo _vitsTurbo;
        
        private readonly AudioSource _audioSource;
        
        private DialogueSystem _system;
        
        public IReadOnlyList<Option> DialogueOptions { get; private set; }
        
        public float MaxWaitTime { get; set; } = 30f;
        
        
        private readonly Dictionary<Option, AudioClip> _audioCacheMap = new();
        
        private readonly ObjectContainer _objectContainer = new();
        
        private readonly CancellationTokenSource _ct = new();
        
        public void Inject(IReadOnlyList<Option> options, DialogueSystem system)
        {
            DialogueOptions = options;
            _system = system;
        }
        
        public async UniTask ClickOption(Option option)
        {
            if (_audioCacheMap.TryGetValue(option, out AudioClip clip))
            {
                _audioSource.clip = clip;
                _audioSource.Play();
                await UniTask.Yield();
                await UniTask.WaitUntil(() => !_audioSource.isPlaying);
            }
            // Handle CallBack Module
            CallBackModule.InvokeCallBack(option);
            if (string.IsNullOrEmpty(option.TargetID))
            {
                // Exit Dialogue
                _system.EndDialogue(false);
            }
            else
            {
                _system.PlayDialoguePiece(option.TargetID);
            }
        }

        public async UniTask EnterOption(CancellationToken cancellationToken)
        {
            _audioCacheMap.Clear();
            foreach (var option in DialogueOptions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                _objectContainer.Register<IContentModule>(option);
                await option.ProcessModules(_objectContainer, cancellationToken);
                if (option.TryGetModule(out VITSModule module))
                {
                    // Use the passed cancellationToken instead of internal _ct.Token
                    _audioCacheMap[option] = await module.RequestOrLoadAudioClip(_vitsTurbo, option.Content, cancellationToken).Timeout(TimeSpan.FromSeconds(MaxWaitTime));
                }
            }
        }
    }
}
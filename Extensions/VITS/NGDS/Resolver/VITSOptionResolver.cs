using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace Kurisu.NGDS.VITS
{
    public class VITSOptionResolver : IOptionResolver
    {
        public VITSOptionResolver(VITSTurbo vitsTurbo, AudioSource audioSource)
        {
            this.vitsTurbo = vitsTurbo;
            this.audioSource = audioSource;
        }
        private readonly VITSTurbo vitsTurbo;
        private readonly AudioSource audioSource;
        private IDialogueSystem system;
        public IReadOnlyList<Option> DialogueOptions { get; private set; }
        public float MaxWaitTime { get; set; } = 30f;
        private readonly Dictionary<Option, AudioClip> audioCacheMap = new();
        private readonly ObjectContainer objectContainer = new();
        private readonly CancellationTokenSource ct = new();
        public void Inject(IReadOnlyList<Option> options, IDialogueSystem system)
        {
            DialogueOptions = options;
            this.system = system;
        }
        public IEnumerator ClickOption(Option option)
        {
            if (audioCacheMap.TryGetValue(option, out AudioClip clip))
            {
                audioSource.clip = clip;
                audioSource.Play();
                yield return null;
                while (audioSource.isPlaying)
                    yield return null;
            }
            //Handle CallBack Module
            CallBackModule.InvokeCallBack(option);
            if (string.IsNullOrEmpty(option.TargetID))
            {
                //Exit Dialogue
                system.EndDialogue();
            }
            else
            {
                system.PlayDialoguePiece(option.TargetID);
            }
        }

        public IEnumerator EnterOption()
        {
            audioCacheMap.Clear();
            foreach (var option in DialogueOptions)
            {
                objectContainer.Register<IContentModule>(option);
                yield return option.ProcessModules(objectContainer);
                if (option.TryGetModule(out VITSModule module))
                {
                    float waitTime = 0;
                    var task = module.RequestOrLoadAudioClip(vitsTurbo, option.Content, ct.Token);
                    while (!task.IsCompleted)
                    {
                        yield return null;
                        waitTime += Time.deltaTime;
                        if (waitTime >= MaxWaitTime)
                        {
                            ct.Cancel();
                            break;
                        }
                    }
                    audioCacheMap[option] = task.Result;
                    continue;
                }
            }
        }
    }
}
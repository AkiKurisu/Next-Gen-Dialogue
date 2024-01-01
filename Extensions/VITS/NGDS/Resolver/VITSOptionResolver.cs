#if NGD_USE_VITS
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Kurisu.NGDS.AI;
using UnityEngine;
namespace Kurisu.NGDS.VITS
{
    public class VITSOptionResolver : IOptionResolver
    {
        public VITSOptionResolver(AIPromptBuilder promptBuilder, VITSTurbo vitsTurbo, AudioSource audioSource)
        {
            this.promptBuilder = promptBuilder;
            objectContainer.Register(promptBuilder);
            this.vitsTurbo = vitsTurbo;
            this.audioSource = audioSource;
        }
        private readonly VITSTurbo vitsTurbo;
        private readonly AudioSource audioSource;
        private IDialogueSystem system;
        private readonly AIPromptBuilder promptBuilder;
        public IReadOnlyList<Option> DialogueOptions { get; private set; }
        public float MaxWaitTime { get; set; } = 30f;
        private readonly Dictionary<Option, AudioClip> audioCacheMap = new();
        private readonly OptionCallBackHandler callBackHandler = new();
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
            if (string.IsNullOrEmpty(option.TargetID))
            {
                //Exit Dialogue
                system.EndDialogue();
            }
            else
            {
                if (option.TryGetModule(out CharacterModule characterModule))
                {
                    promptBuilder.Append(characterModule.CharacterName, option.Content);
                }
                system.PlayDialoguePiece(option.TargetID);
            }
            //Handle CallBack Module
            callBackHandler.Handle(option);
        }

        public IEnumerator EnterOption()
        {
            audioCacheMap.Clear();
            foreach (var option in DialogueOptions)
            {
                objectContainer.Register<IContent>(option);
                for (int i = 0; i < option.Modules.Count; i++)
                {
                    if (option.Modules[i] is IProcessable injectable)
                        yield return injectable.Process(objectContainer);
                }
                if (option.TryGetModule(out VITSAudioClipModule audioClipModule))
                {
                    audioCacheMap[option] = audioClipModule.AudioClip;
                    continue;
                }
                if (option.TryGetModule(out VITSGenerateModule vitsModule))
                {
                    var task = vitsTurbo.SendVITSRequestAsync(option.Content, vitsModule.CharacterID, ct.Token);
                    float waitTime = 0;
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
                    var response = task.Result;
                    if (response.Status)
                    {
                        audioCacheMap[option] = response.Result;
                    }
                    else
                    {
                        Debug.LogWarning("[VITS Option Resolver] VITS Request failed !");
                    }
                }
            }
        }
    }
}
#endif
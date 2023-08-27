#if USE_VITS
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public IReadOnlyList<DialogueOption> DialogueOptions { get; private set; }
        private readonly Dictionary<DialogueOption, AudioClip> audioCacheMap = new();
        private readonly OptionCallBackHandler callBackHandler = new();
        private readonly ObjectContainer objectContainer = new();
        public void Inject(IReadOnlyList<DialogueOption> options, IDialogueSystem system)
        {
            DialogueOptions = options;
            this.system = system;
        }
        public async Task OnOptionClick(DialogueOption option)
        {
            if (audioCacheMap.TryGetValue(option, out AudioClip clip))
            {
                audioSource.clip = clip;
                audioSource.Play();
                await Task.Yield();
                while (audioSource.isPlaying)
                    await Task.Yield();
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

        public async Task OnOptionEnter()
        {
            audioCacheMap.Clear();
            foreach (var option in DialogueOptions)
            {
                objectContainer.Register<IContent>(option);
                for (int i = 0; i < option.Modules.Count; i++)
                {
                    if (option.Modules[i] is IInjectable injectable)
                        await injectable.Inject(objectContainer);
                }
                if (option.TryGetModule(out VITSModule vitsModule))
                {
                    var response = await vitsTurbo.SendVITSRequestAsync(option.Content, vitsModule.CharacterID);
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
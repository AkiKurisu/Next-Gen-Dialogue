using Kurisu.NGDS.VITS;
using Kurisu.NGDT.Editor;
using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.VITS.Editor
{
    [Ordered]
    public class VITSModuleResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new VITSModuleNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(VITSModule);
    }
    public class VITSModuleNode : ModuleNode
    {
        private SharedTObjectField<AudioClip> audioClipField;
        private AudioPreviewField audioPreviewField;
        private bool isBaking;
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Bake Audio", (a) =>
            {
                BakeAudio();
            }, (e) =>
            {
                if (isBaking) return DropdownMenuAction.Status.Disabled;
                else return DropdownMenuAction.Status.Normal;
            }));
        }
        protected override void OnBehaviorSet()
        {
            audioClipField = (GetFieldResolver("audioClip") as SharedTObjectResolver<AudioClip>).EditorField;
        }
        protected override void OnRestore()
        {
            if (audioClipField.value.Value != null)
            {
                audioPreviewField = new AudioPreviewField(audioClipField.value.Value);
                mainContainer.Add(audioPreviewField);
            }
        }
        private async void BakeAudio()
        {
            var characterID = this.GetSharedIntValue(mapTreeView, "characterID");
            if (!GetFirstAncestorOfType<ContainerNode>().TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
            string content = contentModule.GetSharedStringValue(mapTreeView, "content");
            var turboSetting = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
            var vitsTurbo = new VITSTurbo(turboSetting)
            {
                PreTranslateModule = turboSetting.Enable_GoogleTranslation ? new(turboSetting.LLM_Language, turboSetting.VITS_Language) : null
            };
            isBaking = true;
            var response = await vitsTurbo.SendVITSRequestAsync(content, characterID);
            if (response.Status)
            {
                audioPreviewField?.RemoveFromHierarchy();
                audioPreviewField = new AudioPreviewField(response.Result, false, OnDownloadAudioClip);
                mainContainer.Add(audioPreviewField);
            }
            isBaking = false;
        }
        private void OnDownloadAudioClip(AudioClip audioClip)
        {
            if (audioClipField == null) return;
            audioClipField.value.Value = audioClip;
            audioClipField.Repaint();
        }
    }

}

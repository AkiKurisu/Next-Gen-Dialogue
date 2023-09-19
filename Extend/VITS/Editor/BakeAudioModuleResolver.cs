using Kurisu.NGDS.VITS;
using Kurisu.NGDT.Editor;
using System;
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
                audioPreviewField = new AudioPreviewField(response.Result);
                mainContainer.Add(audioPreviewField);
            }
            isBaking = false;
        }
    }

}

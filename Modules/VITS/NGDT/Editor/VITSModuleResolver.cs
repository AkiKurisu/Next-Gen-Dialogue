#if NGD_USE_VITS
using Kurisu.NGDS.VITS;
using Kurisu.NGDT.Editor;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
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
        private class VITSModuleNode : ModuleNode
        {
            private SharedTObjectField<AudioClip> audioClipField;
            private AudioPreviewField audioPreviewField;
            private readonly CancellationTokenSource ct = new();
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
                var characterID = this.GetSharedIntValue("characterID");
                if (!GetFirstAncestorOfType<ContainerNode>().TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
                string content = contentModule.GetSharedStringValue("content");
                var turboSetting = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
                var vitsTurbo = new VITSTurbo(turboSetting)
                {
                    PreTranslateModule = turboSetting.Enable_GoogleTranslation ? new(turboSetting.LLM_Language, turboSetting.VITS_Language) : null
                };
                isBaking = true;
                float startVal = (float)EditorApplication.timeSinceStartup;
                const float maxValue = 60.0f;
                var task = vitsTurbo.SendVITSRequestAsync(content, characterID, ct.Token);
                bool cancel = false;
                while (!task.IsCompleted)
                {
                    float slider = (float)(EditorApplication.timeSinceStartup - startVal) / maxValue;
                    EditorUtility.DisplayProgressBar("Wait to bake audio", "Waiting for a few seconds", slider);
                    if (slider > 1)
                    {
                        MapTreeView.EditorWindow.ShowNotification(new GUIContent($"Audio baking is out of time, please check your internet!"));
                        ct.Cancel();
                        cancel = true;
                        break;
                    }
                    await Task.Yield();
                }
                if (!cancel)
                {
                    if (task.Result.Status)
                    {
                        audioPreviewField?.RemoveFromHierarchy();
                        audioPreviewField = new AudioPreviewField(task.Result.Result, false, OnDownloadAudioClip);
                        mainContainer.Add(audioPreviewField);
                    }
                    else
                    {
                        MapTreeView.EditorWindow.ShowNotification(new GUIContent($"Audio baked failed!"));
                    }
                }
                EditorUtility.ClearProgressBar();
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
}
#endif
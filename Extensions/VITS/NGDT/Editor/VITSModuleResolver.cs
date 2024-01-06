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
        internal class VITSModuleNode : ModuleNode
        {
            private SharedTObjectField<AudioClip> audioClipField;
            private AudioPreviewField audioPreviewField;
            private bool isBaking;
            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                base.BuildContextualMenu(evt);
                evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Bake Audio", async (a) =>
                {
                    await BakeAudio();
                }, (e) =>
                {
                    if (isBaking) return DropdownMenuAction.Status.Disabled;
                    else return DropdownMenuAction.Status.Normal;
                }));
                evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Clean Audio", (a) =>
                {
                    audioClipField.value.Value = null;
                    audioClipField.Repaint();
                }, (e) =>
                {
                    if (ContainsAudioClip()) return DropdownMenuAction.Status.Normal;
                    else return DropdownMenuAction.Status.Disabled;
                }));
                evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Delate Audio", (a) =>
               {
                   if (EditorUtility.DisplayDialog("Warning", $"Delate audioClip {audioClipField.value.Value.name}? This operation cannot be undone.", "Delate", "Cancel"))
                   {
                       AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(audioClipField.value.Value));
                       audioClipField.value.Value = null;
                       audioClipField.Repaint();
                   }
               }, (e) =>
               {
                   if (ContainsAudioClip()) return DropdownMenuAction.Status.Normal;
                   else return DropdownMenuAction.Status.Disabled;
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
            public async Task<bool> BakeAudio()
            {
                var characterID = this.GetSharedIntValue("characterID");
                if (!GetFirstAncestorOfType<ContainerNode>().TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return false;
                string content = contentModule.GetSharedStringValue("content");
                var turboSetting = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
                var vitsTurbo = new VITSTurbo(turboSetting)
                {
                    PreTranslateModule = turboSetting.Enable_GoogleTranslation ? new(turboSetting.LLM_Language, turboSetting.VITS_Language) : null
                };
                isBaking = true;
                float startVal = (float)EditorApplication.timeSinceStartup;
                const float maxValue = 60.0f;
                var ct = MapTreeView.GetCancellationTokenSource();
                var task = vitsTurbo.SendVITSRequestAsync(content, characterID, ct.Token);
                while (!task.IsCompleted)
                {
                    float slider = (float)(EditorApplication.timeSinceStartup - startVal) / maxValue;
                    EditorUtility.DisplayProgressBar("Wait to bake audio", "Waiting for a few seconds", slider);
                    if (slider > 1)
                    {
                        MapTreeView.EditorWindow.ShowNotification(new GUIContent($"Audio baking is out of time, please check your internet!"));
                        ct.Cancel();
                        break;
                    }
                    await Task.Yield();
                }
                if (!task.IsCanceled && task.Result.Status)
                {
                    audioPreviewField?.RemoveFromHierarchy();
                    audioPreviewField = new AudioPreviewField(task.Result.Result, false, OnDownloadAudioClip);
                    mainContainer.Add(audioPreviewField);
                }
                else
                {
                    MapTreeView.EditorWindow.ShowNotification(new GUIContent($"Audio baked failed!"));
                }
                EditorUtility.ClearProgressBar();
                isBaking = false;
                return !task.IsCanceled && task.Result.Status;
            }
            private void OnDownloadAudioClip(AudioClip audioClip)
            {
                if (audioClipField == null) return;
                audioClipField.value.Value = audioClip;
                audioClipField.Repaint();
            }
            public bool ContainsAudioClip()
            {
                if (audioClipField == null) return false;
                return audioClipField.value.Value != null;
            }
            public bool IsSharedMode()
            {
                if (audioClipField == null) return false;
                return audioClipField.value.IsShared;
            }
        }
    }
}
#endif
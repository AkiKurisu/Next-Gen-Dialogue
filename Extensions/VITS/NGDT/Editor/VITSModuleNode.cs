using Kurisu.NGDS.AI;
using Kurisu.NGDS.VITS;
using Kurisu.NGDT.Editor;
using System;
using System.Threading.Tasks;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.VITS.Editor
{
    [CustomNodeView(typeof(VITSModule))]
    public class VITSModuleNode : ModuleNode
    {
        private SharedTObjectField<AudioClip> audioClipField;
        private AudioPreviewField audioPreviewField;
        private bool isBaking;
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Bake Audio", async (a) =>
            {
                await BakeAudio();
            }, (e) =>
            {
                if (isBaking) return DropdownMenuAction.Status.Disabled;
                else return DropdownMenuAction.Status.Normal;
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Clean Audio", (a) =>
            {
                audioClipField.value.Value = null;
                audioClipField.Repaint();
            }, (e) =>
            {
                if (ContainsAudioClip()) return DropdownMenuAction.Status.Normal;
                else return DropdownMenuAction.Status.Disabled;
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Delete Audio", (a) =>
           {
               if (EditorUtility.DisplayDialog("Warning", $"Delete audioClip {audioClipField.value.Value.name}? This operation cannot be undone.", "Delate", "Cancel"))
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
            audioClipField = ((SharedTObjectResolver<AudioClip>)GetFieldResolver("audioClip")).BaseField;
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
            var parent = GetFirstAncestorOfType<ContainerNode>();
            if (parent == null) return false;
            int index = Array.IndexOf(parent.GetModuleNodes<VITSModule>(), this);
            var contentModule = parent.GetModuleNode<ContentModule>(index);
            string content = contentModule.GetSharedStringValue("content");
            var turboSetting = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
            var vitsTurbo = new VITSTurbo(turboSetting)
            {
                Translator = LLMFactory.CreateTranslator(turboSetting.TranslatorType, turboSetting, turboSetting.LLM_Language, turboSetting.VITS_Language)
            };
            isBaking = true;
            float startVal = (float)EditorApplication.timeSinceStartup;
            const float maxValue = 60.0f;
            var ct = MapGraphView.GetCancellationTokenSource();
            Task<VITSResponse> task;
            if (this.GetFieldValue<bool>("noTranslation"))
            {
                task = vitsTurbo.SendVITSRequestAsync(content, characterID, ct.Token);
            }
            else
            {
                task = vitsTurbo.SendVITSRequestAsyncWithTranslation(content, characterID, ct.Token);
            }
            while (!task.IsCompleted)
            {
                float slider = (float)(EditorApplication.timeSinceStartup - startVal) / maxValue;
                EditorUtility.DisplayProgressBar("Wait to bake audio", "Waiting for a few seconds", slider);
                if (slider > 1)
                {
                    MapGraphView.EditorWindow.ShowNotification(new GUIContent($"Audio baking is out of time, please check your internet!"));
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
                MapGraphView.EditorWindow.ShowNotification(new GUIContent($"Audio baked failed!"));
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
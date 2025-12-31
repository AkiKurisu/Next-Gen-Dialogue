using NextGenDialogue.AI;
using NextGenDialogue.VITS;
using System;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Cysharp.Threading.Tasks;
using NextGenDialogue.Graph.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.VITS.Editor
{
    [CustomNodeView(typeof(VITSModule))]
    public class VitsModuleNodeView : ModuleNodeView
    {
        private SharedTObjectField<AudioClip> _audioClipField;
        
        private AudioPreviewField _audioPreviewField;
        
        private bool _isBaking;

        public VitsModuleNodeView(Type type, CeresGraphView graphView) : base(type, graphView)
        {
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Bake Audio", _ =>
            {
                BakeAudio().Forget();
            }, _ => _isBaking ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal));
            
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Clean Audio", _ =>
            {
                _audioClipField.value.Value = null;
                _audioClipField.Repaint();
            }, _ => ContainsAudioClip() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
            
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Delete Audio", _ =>
           {
               if (EditorUtility.DisplayDialog("Warning", $"Delete audioClip {_audioClipField.value.Value.name}? This operation cannot be undone.", "Delete", "Cancel"))
               {
                   AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_audioClipField.value.Value));
                   _audioClipField.value.Value = null;
                   _audioClipField.Repaint();
               }
           }, _ => ContainsAudioClip() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            _audioClipField = ((SharedTObjectResolver<AudioClip>)GetFieldResolver("audioClip")).BaseField;
        }
        
        protected override void OnRestore()
        {
            if (_audioClipField.value.Value != null)
            {
                _audioPreviewField = new AudioPreviewField(_audioClipField.value.Value);
                mainContainer.Add(_audioPreviewField);
            }
        }
        
        public async UniTask<bool> BakeAudio()
        {
            var characterID = this.GetSharedIntValue("characterID");
            var parentNode = GetFirstAncestorOfType<ContainerNodeView>();
            if (parentNode == null) return false;
            int index = Array.IndexOf(parentNode.GetModuleNodes<VITSModule>(), this);
            var contentModule = parentNode.GetModuleNode<ContentModule>(index);
            string content = contentModule.GetSharedStringValue("content");
            var turboSetting = NextGenDialogueSettings.Get().AITurboSetting;
            var vitsTurbo = new VITSTurbo(turboSetting)
            {
                Translator = LLMFactory.CreateTranslator(turboSetting.TranslatorType, turboSetting, turboSetting.LLM_Language, turboSetting.VITS_Language)
            };
            _isBaking = true;
            float startVal = (float)EditorApplication.timeSinceStartup;
            const float maxValue = 60.0f;
            
            var ct = GraphView.GetCancellationTokenSource();
            VITSResponse response = default;
            var task = UniTask.Create(async () =>
            {
                if (this.GetFieldValue<bool>("noTranslation"))
                {
                    response = await vitsTurbo.SendVITSRequestAsync(content, characterID, ct.Token);
                }
                else
                {
                    response = await vitsTurbo.SendVITSRequestAsyncWithTranslation(content, characterID, ct.Token);
                }
                return response;
            });
            
            while (task.Status == UniTaskStatus.Pending)
            {
                float slider = (float)(EditorApplication.timeSinceStartup - startVal) / maxValue;
                EditorUtility.DisplayProgressBar("Wait to bake audio", "Waiting for a few seconds", slider);
                if (slider > 1)
                {
                    GraphView.EditorWindow.ShowNotification(new GUIContent($"Audio baking is out of time, please check your internet!"));
                    ct.Cancel();
                    break;
                }
                await UniTask.Yield();
            }
            
            if (task.Status == UniTaskStatus.Succeeded)
            {
                _audioPreviewField?.RemoveFromHierarchy();
                _audioPreviewField = new AudioPreviewField(response.Result, false, OnDownloadAudioClip);
                mainContainer.Add(_audioPreviewField);
            }
            else
            {
                GraphView.EditorWindow.ShowNotification(new GUIContent($"Audio baked failed!"));
            }
            
            EditorUtility.ClearProgressBar();
            _isBaking = false;
            return task.Status == UniTaskStatus.Succeeded;
        }
        
        private void OnDownloadAudioClip(AudioClip audioClip)
        {
            if (_audioClipField == null) return;
            _audioClipField.value.Value = audioClip;
            _audioClipField.Repaint();
        }
        
        public bool ContainsAudioClip()
        {
            if (_audioClipField == null) return false;
            return _audioClipField.value.Value != null;
        }
        
        public bool IsSharedMode()
        {
            return _audioClipField != null && _audioClipField.value.IsShared;
        }
    }
}
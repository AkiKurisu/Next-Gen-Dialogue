using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Ceres.Graph;
using Chris;
using Cysharp.Threading.Tasks;
using NextGenDialogue;
using UnityEngine;
using UnityEngine.UIElements;
namespace NextGenDialogue.Graph.Editor
{
    [CustomNodeView(typeof(EditorTranslateModule))]
    public class EditorTranslateNodeView : EditorModuleNodeView
    {
        private bool IsPending { get; set; }
        
        public EditorTranslateNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            //The custom contextual menu builder will only activate when this editor node is attached
            Graph.ContextualMenuRegistry.Register<EditorTranslateNodeView>(new TranslateContextualMenuBuilder(this, CanTranslate));
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            //Do not forget to unregister after detach
            Graph.ContextualMenuRegistry.UnRegister<EditorTranslateNodeView>();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Translate All Contents", (a) =>
            {
                TranslateAllContentsAsync();
            }, _ =>
            {
                if (IsPending) return DropdownMenuAction.Status.Disabled;
                return DropdownMenuAction.Status.Normal;
            }));
        }

        private async void TranslateAllContentsAsync()
        {
            IsPending = true;
            string sourceLanguageCode = this.GetFieldValue<string>("sourceLanguageCode");
            string targetLanguageCode = this.GetFieldValue<string>("targetLanguageCode");
            var containerNodes = Graph.nodes
                .OfType<ContainerNodeView>()
                .Where(x => x is not DialogueContainerView)
                .ToArray();
            var tasks = new List<UniTask>(10);
            var ct = new CancellationTokenSource();
            foreach (var node in containerNodes)
            {
                if (node.TryGetModuleNode<ContentModule>(out var moduleNode))
                {
                    tasks.Add(TranslateContentsAsync(node, moduleNode, ct.Token));
                }
                if (tasks.Count == 10)
                {
                    await UniTask.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            if (tasks.Count != 0)
                await UniTask.WhenAll(tasks);
            Graph.EditorWindow.ShowNotification(new GUIContent("Translation Complete !"));
            IsPending = false;
            async UniTask TranslateContentsAsync(ContainerNodeView containerNode, ModuleNodeView moduleNode, CancellationToken ct)
            {
                string input = moduleNode.GetSharedStringValue("content");
                var response = await GoogleTranslateHelper.TranslateTextAsync(sourceLanguageCode, targetLanguageCode, input, ct);
                if (response.Status)
                {
                    ((SharedStringResolver)moduleNode.GetFieldResolver("content")).BaseField.SetValue(response.TranslateText);
                }
            }
        }
        internal async void TranslateNodeAsync(IDialogueNodeView node)
        {
            IsPending = true;
            string sourceLanguageCode = this.GetFieldValue<string>("sourceLanguageCode");
            string targetLanguageCode = this.GetFieldValue<string>("targetLanguageCode");

            var fieldsToTranslate = node.GetBehavior()
                .GetAllFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute(typeof(TranslateEntryAttribute)) != null)
                .ToArray();
            int fieldCount = fieldsToTranslate.Length;
            var tasks = new List<UniTask>(10);
            var ct = new CancellationTokenSource();
            for (int i = 0; i < fieldCount; ++i)
            {
                tasks.Add(TranslateContentsAsync(fieldsToTranslate[i], ct.Token));
                if (tasks.Count == 10)
                {
                    await UniTask.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            if (tasks.Count != 0)
                await UniTask.WhenAll(tasks);
            Graph.EditorWindow.ShowNotification(new GUIContent("Translation Complete !"));
            IsPending = false;
            async UniTask TranslateContentsAsync(FieldInfo fieldInfo, CancellationToken ct)
            {
                string input;
                if (fieldInfo.FieldType == typeof(string))
                    input = node.GetFieldValue<string>(fieldInfo.Name);
                else if (fieldInfo.FieldType == typeof(SharedString))
                    input = node.GetSharedStringValue(fieldInfo.Name);
                else
                {
                    Debug.LogWarning($"Field type of {fieldInfo.FieldType} can not be translated yet, translation was skipped");
                    return;
                }
                var response = await GoogleTranslateHelper.TranslateTextAsync(sourceLanguageCode, targetLanguageCode, input, ct);
                if (response.Status)
                {
                    if (fieldInfo.FieldType == typeof(string))
                    {
                        node.GetFieldResolver(fieldInfo.Name).Value = response.TranslateText;
                    }
                    else
                    {
                        ((SharedStringResolver)node.GetFieldResolver(fieldInfo.Name)).BaseField.SetValue(response.TranslateText);
                    }
                }
            }
        }

        private static bool CanTranslate(Type type)
        {
            if (type == null) return false;
            return type.GetAllFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                       .Any(x => x.GetCustomAttribute(typeof(TranslateEntryAttribute)) != null);
        }
        
        private class TranslateContextualMenuBuilder : ContextualMenuBuilder
        {
            public TranslateContextualMenuBuilder(EditorTranslateNodeView editorTranslateNodeView, Func<Type, bool> canBuildFunc) :
            base(
                ContextualMenuType.Node,
                canBuildFunc,
               evt =>
               {
                   var target = evt.target;
                   evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Translate", a =>
                    {
                        editorTranslateNodeView.TranslateNodeAsync(target as IDialogueNodeView);
                    }, _ =>
                   {
                       if (editorTranslateNodeView.IsPending) return DropdownMenuAction.Status.Disabled;
                       return DropdownMenuAction.Status.Normal;
                   }));
               })
            { }
        }
    }

}
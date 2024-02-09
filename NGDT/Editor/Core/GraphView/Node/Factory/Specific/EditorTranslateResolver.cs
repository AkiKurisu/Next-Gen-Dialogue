using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kurisu.NGDS;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [Ordered]
    public class EditorTranslateResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new EditorTranslateNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(EditorTranslateModule);
        private class EditorTranslateNode : EditorModuleNode
        {
            public bool IsPending { get; private set; }
            public EditorTranslateNode()
            {
                RegisterCallback<AttachToPanelEvent>(OnAttach);
                RegisterCallback<DetachFromPanelEvent>(OnDetach);
            }

            private void OnAttach(AttachToPanelEvent evt)
            {
                //The custom contextual menu builder will only activate when this editor node is attached
                MapTreeView.ContextualMenuController.Register<EditorTranslateNode>(new TranslateContextualMenuBuilder(this, CanTranslate));
            }

            private void OnDetach(DetachFromPanelEvent evt)
            {
                //Do not forget to unregister after detach
                MapTreeView.ContextualMenuController.UnRegister<EditorTranslateNode>();
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                base.BuildContextualMenu(evt);
                evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Translate All Contents", (a) =>
                {
                    TranslateAllContentsAsync();
                }, (e) =>
                {
                    if (IsPending) return DropdownMenuAction.Status.Disabled;
                    else return DropdownMenuAction.Status.Normal;
                }));
            }

            private async void TranslateAllContentsAsync()
            {
                IsPending = true;
                string sourceLanguageCode = this.GetFieldValue<string>("sourceLanguageCode");
                string targetLanguageCode = this.GetFieldValue<string>("targetLanguageCode");
                var containerNodes = MapTreeView.View.nodes
                    .OfType<ContainerNode>()
                    .Where(x => x is not DialogueContainer)
                    .ToArray();
                var tasks = new List<Task>(10);
                var ct = new CancellationTokenSource();
                foreach (var node in containerNodes)
                {
                    if (node.TryGetModuleNode<ContentModule>(out ModuleNode moduleNode))
                    {
                        tasks.Add(TranslateContentsAsync(node, moduleNode, ct.Token));
                    }
                    if (tasks.Count == 10)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                }
                if (tasks.Count != 0)
                    await Task.WhenAll(tasks);
                MapTreeView.EditorWindow.ShowNotification(new GUIContent("Translation Complete !"));
                IsPending = false;
                async Task TranslateContentsAsync(ContainerNode containerNode, ModuleNode moduleNode, CancellationToken ct)
                {
                    string input = moduleNode.GetSharedStringValue("content");
                    var response = await GoogleTranslateHelper.TranslateTextAsync(sourceLanguageCode, targetLanguageCode, input, ct);
                    if (response.Status)
                    {
                        (moduleNode.GetFieldResolver("content") as SharedStringResolver).EditorField.SetValue(response.TranslateText);
                    }
                }
            }
            internal async void TranslateNodeAsync(IDialogueNode node)
            {
                IsPending = true;
                string sourceLanguageCode = this.GetFieldValue<string>("sourceLanguageCode");
                string targetLanguageCode = this.GetFieldValue<string>("targetLanguageCode");

                var fieldsToTranslate = node.GetBehavior()
                    .GetAllFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute(typeof(TranslateEntryAttribute)) != null)
                    .ToArray();
                int fieldCount = fieldsToTranslate.Length;
                var tasks = new List<Task>(10);
                var ct = new CancellationTokenSource();
                for (int i = 0; i < fieldCount; ++i)
                {
                    tasks.Add(TranslateContentsAsync(fieldsToTranslate[i], ct.Token));
                    if (tasks.Count == 10)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                }
                if (tasks.Count != 0)
                    await Task.WhenAll(tasks);
                MapTreeView.EditorWindow.ShowNotification(new GUIContent("Translation Complete !"));
                IsPending = false;
                async Task TranslateContentsAsync(FieldInfo fieldInfo, CancellationToken ct)
                {
                    string input = null;
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
                            (node.GetFieldResolver(fieldInfo.Name) as SharedStringResolver).EditorField.SetValue(response.TranslateText);
                        }
                    }
                }
            }
            internal static bool CanTranslate(Type type)
            {
                if (type == null) return false;
                return type.GetAllFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           .Any(x => x.GetCustomAttribute(typeof(TranslateEntryAttribute)) != null);
            }
        }
        private class TranslateContextualMenuBuilder : ContextualMenuBuilder
        {
            public TranslateContextualMenuBuilder(EditorTranslateNode editorTranslateNode, Func<Type, bool> CanBuildFunc) :
            base(
                ContextualMenuType.Node,
                CanBuildFunc,
               (evt) =>
               {
                   var target = evt.target;
                   evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Translate", (a) =>
                    {
                        editorTranslateNode.TranslateNodeAsync(target as IDialogueNode);
                    }, (e) =>
                    {
                        if (editorTranslateNode.IsPending) return DropdownMenuAction.Status.Disabled;
                        else return DropdownMenuAction.Status.Normal;
                    }));
               })
            { }
        }
    }
}

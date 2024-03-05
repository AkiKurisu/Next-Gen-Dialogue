using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kurisu.NGDS;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    /// <summary>
    /// Editor Novel Generation API
    /// </summary>
    public static class NovelGenerateExtension
    {
        private const float maxWaitSeconds = 60.0f;
        public static async Task AutoGenerateNovel(this IDialogueTreeView treeView)
        {
            var containers = treeView.View.selection.OfType<ContainerNode>().ToList();
            if (containers.Count == 0) return;
            var bakeContainer = containers.Last();
            bakeContainer.TryGetModuleNode<NovelBakeModule>(out ModuleNode novelModule);
            NovelBaker baker = new(LLMType.ChatGPT);
            int depth = (int)novelModule.GetFieldResolver("generateDepth").Value;
            float startVal = (float)EditorApplication.timeSinceStartup;
            var ct = treeView.GetCancellationTokenSource();
            int step = 0;
            var task = Generate(containers, bakeContainer, ct.Token, 0, depth);
            while (!task.IsCompleted)
            {
                float slider = (float)(EditorApplication.timeSinceStartup - startVal) / maxWaitSeconds;
                EditorUtility.DisplayProgressBar($"Wait to bake novel, step {step}", "Waiting for a few seconds", slider);
                if (slider > 1)
                {
                    treeView.EditorWindow.ShowNotification(new GUIContent($"Novel baking is out of time, please check your internet!"));
                    ct.Cancel();
                    break;
                }
                await Task.Yield();
            }
            if (!task.IsCanceled && !task.Result)
            {
                treeView.EditorWindow.ShowNotification(new GUIContent($"Novel baking failed"));
            }
            EditorUtility.ClearProgressBar();
            await Task.Delay(2);
            //Auto layout
            NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(treeView.View, bakeContainer));

            //Start from Piece
            async Task<bool> Generate(IReadOnlyList<ContainerNode> containers, ContainerNode bakeContainer, CancellationToken ct, int currentDepth, int maxDepth)
            {
                step++;
                if (currentDepth >= maxDepth) return true;
                string result = await baker.Bake(containers, novelModule, ct);
                if (string.IsNullOrEmpty(result)) return false;
                Debug.Log(result);
                Dictionary<string, string> options;
                try
                {
                    options = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError("Deserialization failed");
                    throw e;
                }
                var character = baker.GetCharacters().RandomElement();
                foreach (var pair in options)
                {
                    // Create next container
                    var node = treeView.CreateNextContainer(bakeContainer);
                    // Link nodes
                    treeView.ConnectContainerNodes(bakeContainer, node);
                    // Add bake module from script
                    // Random character
                    node.AddModuleNode(new CharacterModule(new SharedString(character)));
                    Debug.Log(pair.Value);
                    node.AddModuleNode(new ContentModule(pair.Value));
                    // Append current bake to last
                    containers = new List<ContainerNode>(containers)
                    {
                        node
                    };
                    if (!await Generate(containers, node, ct, currentDepth + 1, maxDepth)) return false;
                }
                return true;
            }
        }
    }
}
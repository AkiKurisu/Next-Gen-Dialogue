using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ceres.Editor.Graph;
using Kurisu.NGDS;
using Kurisu.NGDS.AI;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class NovelBaker
    {
        private const float maxWaitSeconds = 60.0f;
        private readonly ILargeLanguageModel llm;
        private readonly StringBuilder stringBuilder = new();
        private PromptTemplate currentTemplate;
        public string UserName { get; set; } = "User";
        public string BotName { get; set; } = "Bot";
        public NovelBaker()
        {
            llm = LLMFactory.CreateLLM(NextGenDialogueSettings.Get().AITurboSetting);
        }
        /// <summary>
        /// Bake Novel Content in target container based on user's node selection
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string> Bake(IReadOnlyList<ContainerNode> containerNodes, ModuleNode novelModule, CancellationToken ct)
        {
            stringBuilder.Clear();
            var bakeContainerNode = containerNodes.Last();
            MessageRole role;
            //Add prompt
            if (bakeContainerNode is OptionContainer)
            {
                role = MessageRole.Bot;
                var overrideTemplate = novelModule.GetFieldValue<TextAsset>("overridePiecePrompt");
                if (overrideTemplate != null) currentTemplate = new PromptTemplate(overrideTemplate);
                else currentTemplate = new PromptTemplate("PiecePrompt_Template");
            }
            else
            {
                role = MessageRole.User;
                var overrideTemplate = novelModule.GetFieldValue<TextAsset>("overrideOptionPrompt");
                if (overrideTemplate != null) currentTemplate = new PromptTemplate(overrideTemplate);
                else currentTemplate = new PromptTemplate("OptionPrompt_Template");
            }
            var first = containerNodes.First();
            if (first is DialogueContainer dialogueContainer && dialogueContainer.TryGetModuleNode<NovelPromptModule>(out var promptModule))
            {
                stringBuilder.AppendLine(currentTemplate.Get(new Dictionary<string, object>()
                {
                    {"Option System Prompt",promptModule.GetSharedStringValue("optionSystemPrompt")},
                    {"Piece System Prompt",promptModule.GetSharedStringValue("pieceSystemPrompt")},
                    {"Story Summary",promptModule.GetSharedStringValue("storySummary")},
                    {"Option Count",novelModule.GetFieldValue<int>("optionCount")}
                }));
            }
            else
            {
                stringBuilder.AppendLine(currentTemplate.Get(new Dictionary<string, object>()
                {
                    {"Option Count",novelModule.GetFieldValue<int>("optionCount")}
                }));
            }
            //Append user designed dialogue
            for (int i = 0; i < containerNodes.Count; i++)
            {
                AppendDialogue(containerNodes[i], stringBuilder);
            }
            //Generate dialogue from agent finally
            try
            {
                return await GenerateDialogue(role, ct);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[Novel Baker] Bake was cancelled");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Novel Baker] {ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// Generate a preview bake content for editor test purpose
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="bakeContainerNode"></param>
        /// <returns></returns> <summary>
        public string Preview(IReadOnlyList<ContainerNode> containerNodes, ModuleNode novelModule, ContainerNode bakeContainerNode)
        {
            StringBuilder stringBuilder = new();
            //Add prompt
            MessageRole role;
            if (bakeContainerNode is OptionContainer)
            {
                role = MessageRole.Bot;
                var overrideTemplate = novelModule.GetFieldValue<TextAsset>("overridePiecePrompt");
                if (overrideTemplate != null) currentTemplate = new PromptTemplate(overrideTemplate);
                else currentTemplate = new PromptTemplate("PiecePrompt_Template");
            }
            else
            {
                role = MessageRole.User;
                var overrideTemplate = novelModule.GetFieldValue<TextAsset>("overrideOptionPrompt");
                if (overrideTemplate != null) currentTemplate = new PromptTemplate(overrideTemplate);
                else currentTemplate = new PromptTemplate("OptionPrompt_Template");
            }
            var first = containerNodes.First();
            if (first is DialogueContainer dialogueContainer && dialogueContainer.TryGetModuleNode<NovelPromptModule>(out var promptModule))
            {
                stringBuilder.AppendLine(currentTemplate.Get(new Dictionary<string, object>()
                {
                    {"Option System Prompt",promptModule.GetSharedStringValue("optionSystemPrompt")},
                    {"Piece System Prompt",promptModule.GetSharedStringValue("pieceSystemPrompt")},
                    {"Story Summary",promptModule.GetSharedStringValue("storySummary")},
                    {"Option Count",novelModule.GetFieldValue<int>("optionCount")}
                }));
            }
            else
            {
                stringBuilder.AppendLine(currentTemplate.Get(new Dictionary<string, object>()
                {
                    {"Option Count",novelModule.GetFieldValue<int>("optionCount")}
                }));
            }
            //Append user designed dialogue
            for (int i = 0; i < containerNodes.Count; i++)
            {
                AppendDialogue(containerNodes[i], stringBuilder);
            }
            stringBuilder.Append($"{GetName(role)}:");
            return stringBuilder.ToString();
        }
        private async Task<string> GenerateDialogue(MessageRole role, CancellationToken ct)
        {
            stringBuilder.Append($"{GetName(role)}:");
            var result = (await llm.GenerateAsync(stringBuilder.ToString(), ct)).Response;
            return result.Replace("```json", string.Empty).Replace("```", string.Empty);
        }
        private void AppendDialogue(ContainerNode containerNode, StringBuilder builder)
        {
            MessageRole role = containerNode is OptionContainer ? MessageRole.User : MessageRole.Bot;
            if (!containerNode.TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
            string content = contentModule.GetSharedStringValue("content");
            builder.Append(GetName(role));
            builder.Append(':');
            builder.AppendLine(content);
        }
        private string GetName(MessageRole role)
        {
            return role == MessageRole.User ? UserName : BotName;
        }
        public static async Task AutoGenerateNovel(DialogueGraphView graphView)
        {
            var containers = graphView.selection.OfType<ContainerNode>().ToList();
            if (containers.Count == 0) return;
            var bakeContainer = containers.Last();
            bakeContainer.TryGetModuleNode<NovelBakeModule>(out ModuleNode novelModule);
            NovelBaker baker = new();
            int depth = (int)novelModule.GetFieldResolver("generateDepth").Value;
            float startVal = (float)EditorApplication.timeSinceStartup;
            var ct = graphView.GetCancellationTokenSource();
            int step = 0;
            var task = Generate(containers, bakeContainer, ct.Token, 0, depth);
            while (!task.IsCompleted)
            {
                float slider = (float)(EditorApplication.timeSinceStartup - startVal) / maxWaitSeconds;
                EditorUtility.DisplayProgressBar($"Wait to bake novel, step {step}", "Waiting for a few seconds", slider);
                if (slider > 1)
                {
                    graphView.EditorWindow.ShowNotification(new GUIContent($"Novel baking is out of time, please check your internet!"));
                    ct.Cancel();
                    break;
                }
                await Task.Yield();
            }
            if (!task.IsCanceled && !task.Result)
            {
                graphView.EditorWindow.ShowNotification(new GUIContent($"Novel baking failed"));
            }
            EditorUtility.ClearProgressBar();
            await Task.Delay(2);
            
            //Auto layout
            NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(graphView, bakeContainer));

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
                foreach (var pair in options)
                {
                    // Create next container
                    var node = graphView.CreateNextContainer(bakeContainer);
                    // Link nodes
                    graphView.ConnectContainerNodes(bakeContainer, node);
                    // Add bake module from script
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
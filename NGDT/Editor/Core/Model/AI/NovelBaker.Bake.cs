using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kurisu.NGDS;
using Kurisu.NGDS.AI;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public partial class NovelBaker
    {
        public class Template
        {
            private readonly string templateText;
            public Template(string path)
            {
                templateText = Resources.Load<TextAsset>($"NGDT/Templates/{path}").text;
            }
            public Template(TextAsset textAsset)
            {
                templateText = textAsset.text;
            }
            public string Get(Dictionary<string, object> inputs)
            {
                string output = templateText;
                foreach (var pair in inputs)
                {
                    if (pair.Value is not string)
                    {
                        output = output.Replace($"!<{pair.Key}>!", JsonConvert.SerializeObject(pair.Value));
                    }
                    else
                    {
                        output = output.Replace($"!<{pair.Key}>!", (string)pair.Value);
                    }
                }
                return output;
            }
            public string Get()
            {
                return templateText;
            }
        }
        private readonly GPTAgent agent;
        private readonly HashSet<string> characterCached = new();
        public IEnumerable<string> GetCharacters() => characterCached;
        private readonly StringBuilder stringBuilder = new();
        private Template currentTemplate;
        public NovelBaker()
        {
        }
        public NovelBaker(LLMType llmType)
        {
            Assert.IsTrue(llmType is LLMType.ChatGLM_OpenAI or LLMType.ChatGPT);
            agent = new GPTAgent(LLMFactory.CreateNonModule(llmType, NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting));
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
            characterCached.Clear();
            var bakeContainerNode = containerNodes.Last();
            bakeContainerNode.TryGetModuleNode<CharacterModule>(out var characterModule);
            string characterName = characterModule.GetSharedStringValue("characterName");
            //Add prompt
            if (bakeContainerNode is OptionContainer)
            {
                var overrideTemplate = novelModule.GetFieldValue<TextAsset>("overridePiecePrompt");
                if (overrideTemplate != null) currentTemplate = new Template(overrideTemplate);
                else currentTemplate = new Template("PiecePrompt_Template");
            }
            else
            {
                var overrideTemplate = novelModule.GetFieldValue<TextAsset>("overrideOptionPrompt");
                if (overrideTemplate != null) currentTemplate = new Template(overrideTemplate);
                else currentTemplate = new Template("OptionPrompt_Template");
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
                AppendDialogue(containerNodes[i]);
            }
            //Generate dialogue from agent finally
            try
            {
                return await GenerateDialogue(characterName, ct);
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
        private async Task<string> GenerateDialogue(string characterName, CancellationToken ct)
        {
            characterCached.Remove(characterName);
            stringBuilder.Append($"{characterCached.RandomElement()}:");
            var result = await agent.Inference(stringBuilder.ToString(), ct);
            return result.Replace("```json", string.Empty).Replace("```", string.Empty);
        }
        private void AppendDialogue(ContainerNode containerNode)
        {
            if (!containerNode.TryGetModuleNode<CharacterModule>(out ModuleNode characterModule)) return;
            string characterName = characterModule.GetSharedStringValue("characterName");
            characterCached.Add(characterName);
            if (!containerNode.TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
            string content = contentModule.GetSharedStringValue("content");
            stringBuilder.Append(characterName);
            stringBuilder.Append(':');
            stringBuilder.AppendLine(content);
        }
    }
}
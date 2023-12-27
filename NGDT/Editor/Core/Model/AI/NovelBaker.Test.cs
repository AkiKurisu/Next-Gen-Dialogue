using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public partial class NovelBaker
    {
        /// <summary>
        /// Generate a test bake content for editor test purpose
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="bakeContainerNode"></param>
        /// <returns></returns> <summary>
        public string TestBake(IReadOnlyList<ContainerNode> containerNodes, ModuleNode novelModule, ContainerNode bakeContainerNode)
        {
            StringBuilder stringBuilder = new();
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
                AppendDialogue(containerNodes[i], stringBuilder);
            }
            bakeContainerNode.TryGetModuleNode<CharacterModule>(out ModuleNode moduleNode);
            string bakeCharacterName = moduleNode.GetSharedStringValue("characterName");
            if (characterCached.Contains(bakeCharacterName))
                characterCached.Remove(bakeCharacterName);
            stringBuilder.Append($"{characterCached.RandomElement()}:");
            return stringBuilder.ToString();
        }
        private void AppendDialogue(ContainerNode containerNode, StringBuilder builder)
        {
            if (!containerNode.TryGetModuleNode<CharacterModule>(out ModuleNode characterModule)) return;
            string characterName = characterModule.GetSharedStringValue("characterName");
            if (!containerNode.TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
            string content = contentModule.GetSharedStringValue("content");
            characterCached.Add(characterName);
            builder.Append(characterName);
            builder.Append(':');
            builder.AppendLine(content);
        }
    }
}
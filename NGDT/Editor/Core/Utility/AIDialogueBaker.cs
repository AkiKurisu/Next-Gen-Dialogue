using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Kurisu.NGDS;
using System;
using Kurisu.NGDS.AI;
using System.Text;

namespace Kurisu.NGDT.Editor
{
    public class AIDialogueBaker
    {
        private AIPromptBuilder builder;
        private readonly HashSet<string> characterCached = new();
        private readonly IDialogueTreeView treeView;
        public AIDialogueBaker(IDialogueTreeView treeView)
        {
            this.treeView = treeView;
        }
        /// <summary>
        /// Bake Dialogue Content in target container based on user's node selection
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="bakeContainerNode"></param>
        /// <returns></returns>
        public async Task<bool> Bake(IList<ContainerNode> containerNodes, ContainerNode bakeContainerNode)
        {

            bakeContainerNode.TryGetModuleNode<AIBakeModule>(out ModuleNode aiBakeModule);
            //Set up driver first
            var driver = GetLLMDriver(aiBakeModule);
            //No need to cache history, instance new is better
            builder = new AIPromptBuilder(driver);
            //Append user designed dialogue
            for (int i = 0; i < containerNodes.Count; i++)
            {
                AppendDialogue(containerNodes[i], builder);
            }
            //Generate dialogue from driver finally
            try
            {
                await GenerateDialogue(bakeContainerNode, aiBakeModule);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AI Dialogue Baker] : {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Generate a test bake content for editor test purpose
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="bakeContainerNode"></param>
        /// <returns></returns> <summary>
        internal string TestBake(IList<ContainerNode> containerNodes, ContainerNode bakeContainerNode)
        {
            StringBuilder stringBuilder = new();
            bakeContainerNode.TryGetModuleNode<AIBakeModule>(out ModuleNode aiBakeModule);
            //Set up driver first
            var driver = GetLLMDriver(aiBakeModule);
            //No need to cache history, instance new is better
            builder = new AIPromptBuilder(driver);
            //Append user designed dialogue
            for (int i = 0; i < containerNodes.Count; i++)
            {
                AppendDialogue(containerNodes[i], builder);
            }
            string bakeCharacterName = GetSharedStringValue(aiBakeModule, "characterName");
            if (!string.IsNullOrEmpty(builder.Prompt))
            {
                stringBuilder.Append(builder.Prompt);
                stringBuilder.Append('\n');
            }
            foreach (var param in builder.History)
                stringBuilder.Append($"{param.Character}:{param.Content}\n");
            stringBuilder.Append($"{bakeCharacterName}:\n");
            return stringBuilder.ToString();
        }
        private ILLMDriver GetLLMDriver(ModuleNode aiBakeModule)
        {
            var type = (LLMType)aiBakeModule.GetFieldResolver("llmType").Value;
            var setting = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
            return LLMFactory.CreateNonModule(type, setting);
        }
        private async Task GenerateDialogue(ContainerNode containerNode, ModuleNode aiBakeModule)
        {
            var type = (LLMType)aiBakeModule.GetFieldResolver("llmType").Value;
            var setting = NextGenDialogueSetting.GetOrCreateSettings();
            SharedString bakeCharacterName = GetSharedString(aiBakeModule, "characterName");
            var otherCharacters = characterCached.Where(x => x != bakeCharacterName.Value);
            var response = await builder.Generate(bakeCharacterName.Value);
            if (response.Status)
            {
                containerNode.AddModuleNode(new CharacterModule(bakeCharacterName.Clone() as SharedString));
                containerNode.AddModuleNode(new ContentModule(response.Response));
            }
        }
        private bool TrySetPrompt(ContainerNode containerNode, AIPromptBuilder builder)
        {
            if (containerNode.TryGetModuleNode<PromptModule>(out ModuleNode promptModule))
            {
                var prompt = GetSharedStringValue(promptModule, "prompt");
                builder.SetPrompt(prompt);
                return true;
            }
            else if (containerNode.TryGetModuleNode<CharacterPresetModule>(out ModuleNode presetModule))
            {
                var user_Name = GetSharedStringValue(presetModule, "user_Name");
                var char_name = GetSharedStringValue(presetModule, "char_name");
                var char_persona = GetSharedStringValue(presetModule, "char_persona");
                var world_scenario = GetSharedStringValue(presetModule, "world_scenario");
                builder.SetPrompt(CharacterPresetHelper.GeneratePrompt(user_Name, char_name, char_persona, world_scenario));
                return true;
            }
            return false;
        }
        private void AppendDialogue(ContainerNode containerNode, AIPromptBuilder builder)
        {
            if (containerNode is DialogueContainer && TrySetPrompt(containerNode, builder)) return;
            if (!containerNode.TryGetModuleNode<CharacterModule>(out ModuleNode characterModule)) return;
            string characterName = GetSharedStringValue(characterModule, "characterName");
            if (!characterCached.Contains(characterName))
                characterCached.Add(characterName);
            if (!containerNode.TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
            string content = GetSharedStringValue(contentModule, "content");
            builder.Append(characterName, content);
        }
        private string GetSharedStringValue(DialogueTreeNode dialogueTreeNode, string fieldName)
        {
            var sharedString = GetSharedString(dialogueTreeNode, fieldName);
            return sharedString != null ? treeView.GetSharedVariableValue(sharedString) : string.Empty;
        }
        private SharedString GetSharedString(DialogueTreeNode dialogueTreeNode, string fieldName)
        {
            try
            {
                return (SharedString)dialogueTreeNode.GetFieldResolver(fieldName).Value;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return null;
            }
        }
    }
}

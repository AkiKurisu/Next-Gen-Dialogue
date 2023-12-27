using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Kurisu.NGDS;
using System;
using Kurisu.NGDS.AI;
using System.Threading;
namespace Kurisu.NGDT.Editor
{
    public partial class DialogueBaker
    {
        private AIPromptBuilder builder;
        public AIPromptBuilder GetLastBuilder()
        {
            return builder;
        }
        /// <summary>
        /// Bake Dialogue Content in target container based on user's node selection
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="bakeContainerNode"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> Bake(IReadOnlyList<ContainerNode> containerNodes, ContainerNode bakeContainerNode, CancellationToken ct)
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
                await GenerateDialogue(bakeContainerNode, aiBakeModule, ct);
                return true;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[Dialogue Baker] Bake was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Dialogue Baker] {ex.Message}");
                return false;
            }
        }
        private ILLMDriver GetLLMDriver(ModuleNode aiBakeModule)
        {
            var type = (LLMType)aiBakeModule.GetFieldResolver("llmType").Value;
            var setting = NextGenDialogueSetting.GetOrCreateSettings().AITurboSetting;
            return LLMFactory.CreateNonModule(type, setting);
        }
        private async Task GenerateDialogue(ContainerNode containerNode, ModuleNode aiBakeModule, CancellationToken ct)
        {
            SharedString bakeCharacterName = aiBakeModule.GetSharedVariable<SharedString>("characterName");
            var response = await builder.Generate(bakeCharacterName.Value, ct);
            if (response.Status)
            {
                //Remove Original Module Node since container can only contain one module for each type
                containerNode.RemoveModule<CharacterModule>();
                containerNode.RemoveModule<ContentModule>();
                //Create Output Module Node
                containerNode.AddModuleNode(new CharacterModule(bakeCharacterName.Clone() as SharedString));
                containerNode.AddModuleNode(new ContentModule(response.Response));
            }
        }
        private bool TrySetPrompt(ContainerNode containerNode, out string prompt)
        {
            prompt = null;
            if (containerNode.TryGetModuleNode<PromptModule>(out ModuleNode promptModule))
            {
                prompt = promptModule.GetSharedStringValue("prompt");
                return true;
            }
            else if (containerNode.TryGetModuleNode<PromptPresetModule>(out ModuleNode promptPresetModule))
            {
                var text = promptPresetModule.GetSharedVariableValue<TextAsset>("prompt");
                if (text == null)
                {
                    Debug.LogError($"[Dialogue Baker] Prompt file is empty in {containerNode}");
                    return false;
                }
                prompt = text.text;
                return true;
            }
            else if (containerNode.TryGetModuleNode<CharacterPresetModule>(out ModuleNode presetModule))
            {
                var user_Name = presetModule.GetSharedStringValue("user_Name");
                var char_name = presetModule.GetSharedStringValue("char_name");
                var char_persona = presetModule.GetSharedStringValue("char_persona");
                var world_scenario = presetModule.GetSharedStringValue("world_scenario");
                prompt = CharacterPresetHelper.GeneratePrompt(user_Name, char_name, char_persona, world_scenario);
                return true;
            }
            return false;
        }
        private void AppendDialogue(ContainerNode containerNode, AIPromptBuilder builder)
        {
            if (containerNode is DialogueContainer && TrySetPrompt(containerNode, out string prompt))
            {
                builder.SetPrompt(prompt);
                return;
            }
            if (!containerNode.TryGetModuleNode<CharacterModule>(out ModuleNode characterModule)) return;
            string characterName = characterModule.GetSharedStringValue("characterName");
            if (!containerNode.TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
            string content = contentModule.GetSharedStringValue("content");
            builder.Append(characterName, content);
        }
    }
}

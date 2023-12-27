using System.Collections.Generic;
using System.Text;
namespace Kurisu.NGDT.Editor
{
    public partial class DialogueBaker
    {
        /// <summary>
        /// Generate a test bake content for editor test purpose
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="bakeContainerNode"></param>
        /// <returns></returns> <summary>
        public string TestBake(IReadOnlyList<ContainerNode> containerNodes, ContainerNode bakeContainerNode)
        {
            StringBuilder stringBuilder = new();
            bakeContainerNode.TryGetModuleNode<AIBakeModule>(out ModuleNode aiBakeModule);
            //Append user designed dialogue
            for (int i = 0; i < containerNodes.Count; i++)
            {
                AppendDialogue(containerNodes[i], stringBuilder);
            }
            string bakeCharacterName = aiBakeModule.GetSharedStringValue("characterName");
            stringBuilder.Append($"{bakeCharacterName}:");
            return stringBuilder.ToString();
        }
        private void AppendDialogue(ContainerNode containerNode, StringBuilder builder)
        {
            if (containerNode is DialogueContainer && TrySetPrompt(containerNode, out string prompt))
            {
                builder.Insert(0, '\n');
                builder.Insert(0, prompt);
                return;
            }
            if (!containerNode.TryGetModuleNode<CharacterModule>(out ModuleNode characterModule)) return;
            string characterName = characterModule.GetSharedStringValue("characterName");
            if (!containerNode.TryGetModuleNode<ContentModule>(out ModuleNode contentModule)) return;
            string content = contentModule.GetSharedStringValue("content");
            builder.Append(characterName);
            builder.Append(':');
            builder.AppendLine(content);
        }
    }
}

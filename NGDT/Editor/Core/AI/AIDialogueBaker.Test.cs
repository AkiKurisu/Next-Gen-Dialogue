using System.Collections.Generic;
using Kurisu.NGDS.AI;
using System.Text;
namespace Kurisu.NGDT.Editor
{
    public partial class AIDialogueBaker
    {
        /// <summary>
        /// Generate a test bake content for editor test purpose
        /// </summary>
        /// <param name="containerNodes"></param>
        /// <param name="bakeContainerNode"></param>
        /// <returns></returns> <summary>
        public string TestBake(IList<ContainerNode> containerNodes, ContainerNode bakeContainerNode)
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
            string bakeCharacterName = aiBakeModule.GetSharedStringValue("characterName");
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
    }
}

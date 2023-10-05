using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDT
{
    public class DialogueTreeTemplate
    {
        [SerializeReference]
        private List<SharedVariable> variables;
        [SerializeReference]
        private Root root;
        public List<SharedVariable> Variables => variables;
        public Root Root => root;
        public string TemplateName { get; }
        public DialogueTreeTemplate(IDialogueTree behaviorTree)
        {
            TemplateName = behaviorTree.Object.name;
            variables = new List<SharedVariable>();
            foreach (var variable in behaviorTree.SharedVariables)
            {
                variables.Add(variable.Clone() as SharedVariable);
            }
            root = behaviorTree.Root;
        }
    }
}

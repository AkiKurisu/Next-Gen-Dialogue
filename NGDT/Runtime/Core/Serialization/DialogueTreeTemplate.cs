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
#if UNITY_EDITOR
        [SerializeField]
        private List<GroupBlockData> blockData;
        public List<GroupBlockData> BlockData => blockData;
#endif
        public DialogueTreeTemplate(IDialogueTree dt)
        {
            TemplateName = dt.Object.name;
            variables = new List<SharedVariable>();
            foreach (var variable in dt.SharedVariables)
            {
                variables.Add(variable.Clone() as SharedVariable);
            }
            root = dt.Root;
#if UNITY_EDITOR
            blockData = new List<GroupBlockData>(dt.BlockData);
#endif
        }
    }
}

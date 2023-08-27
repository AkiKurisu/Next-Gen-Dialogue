using UnityEngine;
namespace Kurisu.NGDT
{
    public static class DialogueTreeExtension
    {
        public static SharedVariable GetSharedVariable(this IDialogueTree dialogueTree, string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                Debug.LogError($"Shared variable name cannot be empty", dialogueTree._Object);
                return null;
            }
            foreach (var variable in dialogueTree.SharedVariables)
            {
                if (variable.Name.Equals(variableName))
                {
                    return variable;
                }
            }
            Debug.LogError($"Can't find shared variable : {variableName}", dialogueTree._Object);
            return null;
        }
    }
}
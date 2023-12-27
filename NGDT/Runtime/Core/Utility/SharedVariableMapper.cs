#if NGDT_REFLECTION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Kurisu.NGDT
{
    public class SharedVariableMapper
    {
        private static readonly Dictionary<Type, List<FieldInfo>> variableLookup = new();
        /// <summary>
        /// Traverse the dialogue tree and automatically init all shared variables
        /// </summary>
        /// <param name="dialogueTree"></param>
        public static void Traverse(IDialogueTree dialogueTree)
        {
            foreach (var behavior in dialogueTree.Traverse())
            {
                var behaviorType = behavior.GetType();
                if (!variableLookup.TryGetValue(behaviorType, out var fields))
                {
                    fields = behaviorType
                            .GetAllFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Where(x => x.FieldType.IsSubclassOf(typeof(SharedVariable)))
                            .ToList();
                    variableLookup.Add(behaviorType, fields);
                }
                foreach (var fieldInfo in fields)
                {
                    (fieldInfo.GetValue(behavior) as SharedVariable).MapToInternal(dialogueTree);
                }
            }
        }
    }
}
#endif
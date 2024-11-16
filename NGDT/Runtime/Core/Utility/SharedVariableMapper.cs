using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres;
namespace Kurisu.NGDT
{
    public class SharedVariableMapper
    {
        private static readonly Dictionary<Type, List<FieldInfo>> fieldInfoLookup = new();
        /// <summary>
        /// Traverse the dialogue tree and automatically init all shared variables
        /// </summary>
        /// <param name="dialogueTree"></param>
        public static void Traverse(IDialogueTree dialogueTree)
        {
            foreach (var behavior in dialogueTree.Root)
            {
                var behaviorType = behavior.GetType();
                if (!fieldInfoLookup.TryGetValue(behaviorType, out var fields))
                {
                    fields = behaviorType
                            .GetAllFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Where(x => x.FieldType.IsSubclassOf(typeof(SharedVariable)))
                            .ToList();
                    fieldInfoLookup.Add(behaviorType, fields);
                }
                foreach (var fieldInfo in fields)
                {
                    (fieldInfo.GetValue(behavior) as SharedVariable).MapTo(dialogueTree);
                }
            }
        }
    }
}
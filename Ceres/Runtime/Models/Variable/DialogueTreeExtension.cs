using UnityEngine;
namespace Ceres
{
    public static class SharedVariableExtension
    {
        /// <summary>
        /// Get shared variable by it's name
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <returns></returns>
        public static SharedVariable GetSharedVariable(this IVariableSource variableScope, string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                return null;
            }
            foreach (var variable in variableScope.SharedVariables)
            {
                if (variable.Name.Equals(variableName))
                {
                    return variable;
                }
            }
            return null;
        }
        /// <summary>
        /// Try get shared variable by it's name
        /// </summary>
        /// <param name="variableScope"></param>
        /// <param name="variableName"></param>
        /// <param name="sharedVariable"></param>
        /// <returns></returns>
        public static bool TryGetSharedVariable(this IVariableSource variableScope, string variableName, out SharedVariable sharedVariable)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                sharedVariable = null;
                return false;
            }
            foreach (var variable in variableScope.SharedVariables)
            {
                if (variable.Name.Equals(variableName))
                {
                    sharedVariable = variable;
                    return true;
                }
            }
            sharedVariable = null;
            return false;
        }
        public static bool TryGetSharedVariable<T>(this IVariableSource variableScope, string variableName, out SharedVariable<T> sharedTVariable) where T : unmanaged
        {
            if (variableScope.TryGetSharedVariable(variableName, out SharedVariable sharedVariable))
            {
                sharedTVariable = sharedVariable as SharedVariable<T>;
                return sharedTVariable != null;
            }
            sharedTVariable = null;
            return false;
        }
        public static bool TryGetSharedString(this IVariableSource variableScope, string variableName, out SharedVariable<string> sharedTVariable)
        {
            if (variableScope.TryGetSharedVariable(variableName, out SharedVariable sharedVariable))
            {
                sharedTVariable = sharedVariable as SharedVariable<string>;
                return sharedTVariable != null;
            }
            sharedTVariable = null;
            return false;
        }
        public static bool TryGetSharedObject(this IVariableSource variableScope, string variableName, out SharedVariable<Object> sharedObject)
        {
            if (variableScope.TryGetSharedVariable(variableName, out SharedVariable sharedVariable))
            {
                sharedObject = sharedVariable as SharedVariable<Object>;
                return sharedObject != null;
            }
            sharedObject = null;
            return false;
        }
        public static bool TryGetSharedObject<T>(this IVariableSource variableScope, string variableName, out SharedVariable<T> sharedTObject) where T : Object
        {
            if (variableScope.TryGetSharedVariable(variableName, out SharedVariable sharedVariable))
            {
                sharedTObject = sharedVariable as SharedVariable<T>;
                return sharedTObject != null;
            }
            sharedTObject = null;
            return false;
        }
        /// <summary>
        /// Map variable to global variables
        /// </summary>
        /// <param name="variableSource"></param>
        public static void MapGlobal(this IVariableSource variableSource)
        {
            var globalVariables = GlobalVariables.Instance;
            foreach (var variable in variableSource.SharedVariables)
            {
                if (!variable.IsGlobal) continue;
                variable.MapTo(globalVariables);
            }
        }
        /// <summary>
        /// Map variable to target variable source
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="variableSource"></param>
        public static void MapTo(this SharedVariable variable, IVariableSource variableSource)
        {
            if (variable == null) return;
            if (!variable.IsShared && !variable.IsGlobal) return;
            if (!variableSource.TryGetSharedVariable(variable.Name, out SharedVariable sharedVariable))
            {
                Debug.LogWarning($"Can not map {variable.Name} to {variableSource} !");
                return;
            }
            variable.Bind(sharedVariable);
        }
    }
}
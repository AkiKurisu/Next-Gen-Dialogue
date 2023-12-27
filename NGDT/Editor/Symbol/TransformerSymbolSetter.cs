using System;
using UnityEditor;
namespace Kurisu.NGDT.Editor
{
    public static class TransformerSymbolSetter
    {
        private const string DEFINE = "NGD_USE_TRANSFORMER";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update += SymbolSet;
        }
        private static void SymbolSet()
        {
            EditorApplication.update -= SymbolSet;
            int depdencyCollect = 0;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "HuggingFace.SharpTransformers") ++depdencyCollect;
                if (assembly.GetName().Name == "Unity.Sentis") ++depdencyCollect;
            }
            if (depdencyCollect >= 2)
            {
                ScriptingSymbolHelper.AddScriptingSymbol(DEFINE);
            }
            else
            {
                ScriptingSymbolHelper.RemoveScriptingSymbol(DEFINE);
            }
        }
    }
}

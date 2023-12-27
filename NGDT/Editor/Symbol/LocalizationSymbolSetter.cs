using System;
using System.Linq;
using UnityEditor;
namespace Kurisu.NGDT.Editor
{
    public static class LocalizationSymbolSetter
    {
        private const string DEFINE = "NGD_USE_LOCALIZATION";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update += SymbolSet;
        }
        private static void SymbolSet()
        {
            EditorApplication.update -= SymbolSet;
            if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "Unity.Localization"))
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

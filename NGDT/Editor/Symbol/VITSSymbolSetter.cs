using System;
using System.Linq;
using UnityEditor;
namespace Kurisu.NGDT.Editor
{
    public static class VITSSymbolSetter
    {
        private const string DEFINE = "NGD_USE_VITS";
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update += SymbolSet;
        }
        private static void SymbolSet()
        {
            EditorApplication.update -= SymbolSet;
            if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "Kurisu.NGDT.VITS"))
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

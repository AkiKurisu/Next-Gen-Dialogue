using System;
using System.Linq;
using UnityEditor;
namespace Kurisu.NGDT.Editor
{
    public static class VITSSymbolSetter
    {
        private const string DEFINE = "USE_VITS";
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
                EditorUtils.AddScriptingSymbol(DEFINE);
            }
            else
            {
                EditorUtils.RemoveScriptingSymbol(DEFINE);
            }
        }
    }
}

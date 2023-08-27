using System;
using System.Linq;
using UnityEditor;
namespace Kurisu.NGDT.Editor
{
    public static class VITSSymbolSetter
    {
        const string DEFINE = "USE_VITS";

        [InitializeOnLoadMethod]
        private static void Init()
        {
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

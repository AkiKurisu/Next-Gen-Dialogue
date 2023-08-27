using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
namespace Kurisu.NGDT.Editor
{
    public static class LocalizationSymbolSetter
    {
        const string DEFINE = "USE_LOCALIZATION";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "Unity.Localization"))
            {
                EditorUtils.AddScriptingSymbol(DEFINE);
            }
            else
            {
                EditorUtils.RemoveScriptingSymbol(DEFINE);
            }
        }
    }
    public static class EditorUtils
    {
        private static NamedBuildTarget GetActiveNamedBuildTarget()
        {
            var buildTargetGroup = GetActiveBuildTargetGroup();
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            return namedBuildTarget;
        }

        private static BuildTargetGroup GetActiveBuildTargetGroup()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

            return buildTargetGroup;
        }

        public static void AddScriptingSymbol(string define)
        {
            var namedBuildTarget = GetActiveNamedBuildTarget();
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            var defineList = defines.ToList();

            if (!defineList.Contains(define))
            {
                defineList.Add(define);
            }
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defineList.ToArray());
        }
        public static void RemoveScriptingSymbol(string define)
        {
            var namedBuildTarget = GetActiveNamedBuildTarget();
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            var defineList = defines.ToList();

            if (defineList.Contains(define))
            {
                defineList.Remove(define);
            }
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defineList.ToArray());
        }
    }
}

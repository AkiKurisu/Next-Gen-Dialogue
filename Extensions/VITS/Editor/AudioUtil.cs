using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace NextGenDialogue.Graph.VITS.Editor
{
    public static class AudioUtil
    {
        public static readonly string PrefKey = Application.productName + "_NGD_AudioSavePath";
        static readonly Dictionary<string, MethodInfo> methods = new();

        static MethodInfo GetMethod(string methodName, Type[] argTypes)
        {
            if (methods.TryGetValue(methodName, out MethodInfo method)) return method;

            var asm = typeof(AudioImporter).Assembly;
            var audioUtil = asm.GetType("UnityEditor.AudioUtil");
            method = audioUtil.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public,
                null,
                argTypes,
                null);

            if (method != null)
            {
                methods.Add(methodName, method);
            }

            return method;
        }


        public static void PlayClip(AudioClip clip)
        {
            if (!clip) return;
#if UNITY_2020_1_OR_NEWER
            var method = GetMethod("PlayPreviewClip", new Type[] { typeof(AudioClip), typeof(int), typeof(bool) });
            method.Invoke(null, new object[] { clip, 0, false });
#else
            var method = GetMethod("PlayClip", new Type[] { typeof(AudioClip) });
            method.Invoke(null, new object[] { clip });
#endif
        }

        public static void StopClip(AudioClip clip)
        {
#if UNITY_2020_1_OR_NEWER
            var method = GetMethod("StopAllPreviewClips", new Type[] { });
            method.Invoke(null, new object[] { });
#else
            if (!clip) return;
            var method = GetMethod("StopClip", new Type[] { typeof(AudioClip) });
            method.Invoke(null, new object[] { clip });
#endif
        }
    }
}

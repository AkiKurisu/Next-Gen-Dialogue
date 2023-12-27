using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kurisu.NGDT.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Kurisu.NGDS.Example.Editor
{
    public class NGDWelcomeWindow : EditorWindow
    {
        private List<string> dialogueScenes;
#if NGD_USE_LOCALIZATION
        private List<string> localizationScenes;
#endif
#if NGD_USE_VITS
        private List<string> vitsScenes;
#endif
#if NGD_USE_TRANSFORMER
        private List<string> transformerScenes;
#endif
        private static bool dontShowOnLoad;
        private static string DontShowOnLoad => Application.productName + ".NGDS.DontShowOnLoad";
        private static string rootPath;
        [InitializeOnLoadMethod]
        private static void SetWelcome()
        {
            dontShowOnLoad = EditorPrefs.GetBool(DontShowOnLoad, false);
            if (!SessionState.GetBool("FirstLoad", false))
            {
                if (!dontShowOnLoad)
                    EditorApplication.update += ShowWelcome;
                SessionState.SetBool("FirstLoad", true);
            }
        }
        [MenuItem("Tools/Next Gen Dialogue/Welcome Window")]
        public static void ShowWelcome()
        {
            EditorApplication.update -= ShowWelcome;
            var window = CreateInstance<NGDWelcomeWindow>();
            window.minSize = new Vector2(500, 700);
            window.maxSize = new Vector2(500, 700);
            window.titleContent = new GUIContent("NGD Welcome Window");
            window.Show();
            window.Focus();
        }
        private string GetRootPath()
        {
            string path = AssetDatabase.GetAssetPath(NextGenDialogueSetting.GetOrCreateSettings());
            while (!path.EndsWith("Next Gen Dialogue"))
            {
                path = GetParentPath(path);
                if (string.IsNullOrEmpty(path)) return null;
            }
            return path;
        }
        private static string GetParentPath(string path)
        {
            int index = path.LastIndexOf('/');
            if (index < 0) return null;
            return path[..index];
        }
        private void OnEnable()
        {
            rootPath = GetRootPath();
            if (string.IsNullOrEmpty(rootPath)) return;
            dialogueScenes = GetScenes($"{rootPath}/Example");
#if NGD_USE_LOCALIZATION
            localizationScenes = GetScenes($"{rootPath}/Modules/Localization");
#endif
#if NGD_USE_VITS
            vitsScenes = GetScenes($"{rootPath}/Modules/VITS");
#endif
#if NGD_USE_TRANSFORMER
            transformerScenes = GetScenes($"{rootPath}/Modules/Transformer");
#endif
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            GUIStyle titleStyle = new(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("Welcome to Next Gen Dialogue !", titleStyle);
            if (!string.IsNullOrEmpty(rootPath))
            {
                DrawExampleScenes();
            }
            else
            {
                GUILayout.Label("Can not found example folder!");
            }
            GUILayout.FlexibleSpace();
            bool newValue;
            if (newValue = GUILayout.Toggle(dontShowOnLoad, "Don't show window on load"))
            {
                dontShowOnLoad = newValue;
                EditorPrefs.SetBool(DontShowOnLoad, dontShowOnLoad);
            }
            GUILayout.Label("Author : AkiKurisu");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("GitHub Page"))
            {
                Application.OpenURL("https://github.com/AkiKurisu");
            }
            if (GUILayout.Button("BiliBili Page"))
            {
                Application.OpenURL("https://space.bilibili.com/20472331");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        private void DrawExampleScenes()
        {
            GUILayout.Box(AssetDatabase.LoadAssetAtPath<Texture>($"{rootPath}/Example/Images/Splash.png"), GUILayout.Height(200), GUILayout.Width(500));
            GUILayout.Label("Documents");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open README"))
            {
                Application.OpenURL(Application.dataPath + $"{rootPath}/User Manual.pdf"[6..]);
            }
            if (GUILayout.Button("Open API"))
            {
                Application.OpenURL(Application.dataPath + $"{rootPath}/API Document.pdf"[6..]);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Dialogue Example Scene");
            foreach (var scene in dialogueScenes)
            {
                if (GUILayout.Button(new FileInfo(scene).Name))
                {
                    EditorSceneManager.OpenScene(scene);
                }
            }
#if NGD_USE_LOCALIZATION
            GUILayout.Label("Localization Example Scenes");
            foreach (var scene in localizationScenes)
            {
                if (GUILayout.Button(new FileInfo(scene).Name))
                {
                    EditorSceneManager.OpenScene(scene);
                }
            }
#endif
#if NGD_USE_VITS
            GUILayout.Label("VITS Example Scenes");
            foreach (var scene in vitsScenes)
            {
                if (GUILayout.Button(new FileInfo(scene).Name))
                {
                    EditorSceneManager.OpenScene(scene);
                }
            }
#endif
#if NGD_USE_TRANSFORMER
            GUILayout.Label("Transformer Example Scenes");
            foreach (var scene in transformerScenes)
            {
                if (GUILayout.Button(new FileInfo(scene).Name))
                {
                    EditorSceneManager.OpenScene(scene);
                }
            }
#endif
        }
        private static List<string> GetScenes(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.unity", SearchOption.AllDirectories).ToList();
        }
    }
}
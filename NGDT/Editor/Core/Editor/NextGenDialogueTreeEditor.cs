using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
namespace Kurisu.NGDT.Editor
{
    internal class DialogueTreeDebugButton : Button
    {
        private const string ButtonText = "Edit DialogueTree";
        private const string DebugText = "Debug DialogueTree";
        public DialogueTreeDebugButton(IDialogueTree tree) : base(() => DialogueEditorWindow.Show(tree))
        {
            style.fontSize = 15;
            style.unityFontStyleAndWeight = FontStyle.Bold;
            style.color = Color.white;
            if (!Application.isPlaying)
            {
                style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
                text = ButtonText;
            }
            else
            {
                text = DebugText;
                style.backgroundColor = new StyleColor(new Color(253 / 255f, 163 / 255f, 255 / 255f));
            }
        }
    }
    internal class DialogueTreePlayButton : Button
    {
        private const string ButtonText = "Play Dialogue";
        public DialogueTreePlayButton(NextGenDialogueTree tree) : base(() => tree.PlayDialogue())
        {
            style.fontSize = 15;
            style.unityFontStyleAndWeight = FontStyle.Bold;
            style.color = Color.white;
            style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
            text = ButtonText;
        }
    }

    [CustomEditor(typeof(NextGenDialogueTree))]
    public class NextGenDialogueTreeEditor : UnityEditor.Editor
    {
        private static readonly string LabelText = $"Next-Gen DialogueTree <size=12>{NextGenDialogueSetting.Version}</size>";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var tree = target as NextGenDialogueTree;
            var label = new Label(LabelText);
            label.style.fontSize = 20;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSetting.GetInspectorStyle());
            var field = new PropertyField(serializedObject.FindProperty("externalDialogueTree"), "External Dialogue Tree");
            myInspector.Add(field);
            if (tree.SharedVariables.Count(x => x.IsExposed) != 0)
            {
                myInspector.Add(new SharedVariablesFoldout(tree, target, this));
            }
            myInspector.Add(new DialogueTreeDebugButton(tree));
            var playButton = new DialogueTreePlayButton(tree);
            playButton.SetEnabled(Application.isPlaying);
            myInspector.Add(playButton);
            return myInspector;
        }
    }
    [CustomEditor(typeof(NextGenDialogueTreeSO))]
    public class NextGenDialogueTreeSOEditor : UnityEditor.Editor
    {
        private static readonly string LabelText = $"Next-Gen DialogueTreeSO <size=12>{NextGenDialogueSetting.Version}</size>";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var tree = target as NextGenDialogueTreeSO;
            var label = new Label(LabelText);
            label.style.fontSize = 20;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSetting.GetInspectorStyle());
            myInspector.Add(new Label("Editor Description"));
            var description = new TextField(string.Empty)
            {
                multiline = true
            };
            description.style.minHeight = 60;
            description.BindProperty(serializedObject.FindProperty("description"));
            myInspector.Add(description);
            if (tree.SharedVariables.Count(x => x.IsExposed) != 0)
            {
                myInspector.Add(new SharedVariablesFoldout(tree, target, this));
            }
            myInspector.Add(new DialogueTreeDebugButton(tree));
            return myInspector;
        }
    }

}
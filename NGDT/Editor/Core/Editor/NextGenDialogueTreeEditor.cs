using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
namespace Kurisu.NGDT.Editor
{
    [CustomEditor(typeof(NextGenDialogueTree))]
    public class NextGenDialogueTreeEditor : UnityEditor.Editor
    {
        private static readonly string LabelText = $"Next-Gen DialogueTree <size=12>{NextGenDialogueSetting.Version}</size>";
        private const string ButtonText = "Edit DialogueTree";
        private const string DebugText = "Debug DialogueTree";
        private VisualElement myInspector;
        public override VisualElement CreateInspectorGUI()
        {
            myInspector = new VisualElement();
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
            var button = DialogueTreeEditorUtility.GetButton(() => DialogueEditorWindow.Show(tree));
            if (!Application.isPlaying)
            {
                button.style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
                button.text = ButtonText;
            }
            else
            {
                button.text = DebugText;
                button.style.backgroundColor = new StyleColor(new Color(253 / 255f, 163 / 255f, 255 / 255f));
            }
            myInspector.Add(button);
            var playButton = DialogueTreeEditorUtility.GetButton(() => tree.PlayDialogue());
            playButton.style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
            playButton.text = "Play Dialogue";
            playButton.SetEnabled(Application.isPlaying);
            myInspector.Add(playButton);
            return myInspector;
        }
    }
    [CustomEditor(typeof(NextGenDialogueTreeSO))]
    public class NextGenDialogueTreeSOEditor : UnityEditor.Editor
    {
        private static readonly string LabelText = $"Next-Gen DialogueTreeSO <size=12>{NextGenDialogueSetting.Version}</size>";
        private const string ButtonText = "Edit DialogueTreeSO";
        private const string DebugText = "Debug DialogueTree";
        private VisualElement myInspector;
        public override VisualElement CreateInspectorGUI()
        {
            myInspector = new VisualElement();
            var tree = target as IDialogueTree;
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
            description.BindProperty(serializedObject.FindProperty("Description"));
            myInspector.Add(description);
            if (tree.SharedVariables.Count(x => x.IsExposed) != 0)
            {
                myInspector.Add(new SharedVariablesFoldout(tree, target, this));
            }
            var button = DialogueTreeEditorUtility.GetButton(() => { DialogueEditorWindow.Show(tree); });
            if (!Application.isPlaying)
            {
                button.style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
                button.text = ButtonText;
            }
            else
            {
                button.text = DebugText;
                button.style.backgroundColor = new StyleColor(new Color(253 / 255f, 163 / 255f, 255 / 255f));
            }
            myInspector.Add(button);
            return myInspector;
        }
    }

}
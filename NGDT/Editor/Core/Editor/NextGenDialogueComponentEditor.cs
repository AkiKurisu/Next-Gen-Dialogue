using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
namespace Kurisu.NGDT.Editor
{
    internal class DialogueTreeDebugButton : Button
    {
        private const string ButtonText = "Edit Dialogue";
        private const string DebugText = "Debug Dialogue";
        public DialogueTreeDebugButton(IDialogueContainer tree) : base(() => DialogueEditorWindow.Show(tree))
        {
            style.fontSize = 15;
            style.unityFontStyleAndWeight = FontStyle.Bold;
            style.color = Color.white;
            if (!Application.isPlaying)
            {
                style.backgroundColor = new StyleColor(new Color(85 / 255f, 205 / 255f, 115 / 255f));
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
        public DialogueTreePlayButton(NextGenDialogueComponent tree) : base(() => tree.GetDialogueGraph().PlayDialogue(tree.gameObject))
        {
            style.fontSize = 15;
            style.unityFontStyleAndWeight = FontStyle.Bold;
            style.color = Color.white;
            style.backgroundColor = new StyleColor(new Color(85 / 255f, 205 / 255f, 115 / 255f));
            text = ButtonText;
        }
    }

    [CustomEditor(typeof(NextGenDialogueComponent))]
    public class NextGenDialogueComponentEditor : UnityEditor.Editor
    {
        private static readonly string LabelText = $"Next-Gen Dialogue <size=12>{NextGenDialogueSetting.Version}</size>";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var tree = target as NextGenDialogueComponent;
            var label = new Label(LabelText);
            label.style.fontSize = 20;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSetting.GetInspectorStyle());
            var field = new PropertyField(serializedObject.FindProperty("externalDialogueAsset"), "External Dialogue Asset");
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
    [CustomEditor(typeof(NextGenDialogueAsset))]
    public class NextGenDialogueAssetEditor : UnityEditor.Editor
    {
        private static readonly string LabelText = $"Next-Gen Dialogue <size=12>{NextGenDialogueSetting.Version}</size>";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var tree = target as NextGenDialogueAsset;
            var label = new Label(LabelText);
            label.style.fontSize = 20;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSetting.GetInspectorStyle());
            myInspector.Add(new Label("Asset Description"));
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
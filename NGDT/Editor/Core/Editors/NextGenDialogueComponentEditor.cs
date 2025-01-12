using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using UEditor = UnityEditor.Editor;
namespace Kurisu.NGDT.Editor
{
    public class NextGenDialogueEditor: UEditor
    {
        internal class DialogueGraphButton : Button
        {
            private const string ButtonText = "Open in Dialogue Graph";
            
            public DialogueGraphButton(IDialogueGraphContainer tree) : base(() => DialogueEditorWindow.Show(tree))
            {
                style.fontSize = 15;
                style.unityFontStyleAndWeight = FontStyle.Bold;
                style.color = Color.white;
                style.backgroundColor = new StyleColor(new Color(85 / 255f, 205 / 255f, 115 / 255f));
                text = ButtonText;
            }
        }
        
        internal class DialogueGraphPlayDialogueButton : Button
        {
            private const string ButtonText = "Play Dialogue";
            
            public DialogueGraphPlayDialogueButton(NextGenDialogueComponent component) : base(component.PlayDialogue)
            {
                style.fontSize = 15;
                style.unityFontStyleAndWeight = FontStyle.Bold;
                style.color = Color.white;
                style.backgroundColor = new StyleColor(new Color(85 / 255f, 205 / 255f, 115 / 255f));
                text = ButtonText;
            }
        }
        
        protected IDialogueGraphContainer Target => (IDialogueGraphContainer)target;
        
        protected static readonly string LabelText = $"Next-Gen Dialogue <size=12>{NextGenDialogueSettings.Version}</size>";
    }

    [CustomEditor(typeof(NextGenDialogueComponent))]
    public class NextGenDialogueComponentEditor : NextGenDialogueEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var label = new Label(LabelText)
            {
                style =
                {
                    fontSize = 20,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSettings.GetInspectorStyle());
            var field = new PropertyField(serializedObject.FindProperty("externalAsset"), "External Asset");
            myInspector.Add(field);
            // create instance for edit
            var instance = (DialogueGraph)Target.GetGraph();
            if (instance.variables.Count(x => x.IsExposed) != 0)
            {
                myInspector.Add(new SharedVariablesFoldout(instance.BlackBoard, () =>
                {
                    // Not serialize data in playing mode
                    if (Application.isPlaying) return;
                    Target.SetGraphData(instance.GetData());
                    EditorUtility.SetDirty(target);
                }));
            }
            myInspector.Add(new DialogueGraphButton(Target));
            var playButton = new DialogueGraphPlayDialogueButton((NextGenDialogueComponent)Target);
            playButton.SetEnabled(Application.isPlaying);
            myInspector.Add(playButton);
            return myInspector;
        }
    }
    
    [CustomEditor(typeof(NextGenDialogueGraphAsset))]
    public class NextGenDialogueAssetEditor : NextGenDialogueEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var label = new Label(LabelText)
            {
                style =
                {
                    fontSize = 20,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSettings.GetInspectorStyle());
            myInspector.Add(new Label("Asset Description"));
            var description = new TextField(string.Empty)
            {
                multiline = true,
                style =
                {
                    minHeight = 60
                }
            };
            description.BindProperty(serializedObject.FindProperty("description"));
            myInspector.Add(description);
            // create instance for edit
            var instance = (DialogueGraph)Target.GetGraph();
            if (instance.variables.Count(x => x.IsExposed) != 0)
            {
                myInspector.Add(new SharedVariablesFoldout(instance.BlackBoard, () =>
                {
                    // Not serialize data in playing mode
                    if (Application.isPlaying) return;
                    Target.SetGraphData(instance.GetData());
                    EditorUtility.SetDirty(target);
                }));
            }
            myInspector.Add(new DialogueGraphButton(Target));
            return myInspector;
        }
    }
}
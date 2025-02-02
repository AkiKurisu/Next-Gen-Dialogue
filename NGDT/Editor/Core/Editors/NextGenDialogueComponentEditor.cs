using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using Ceres.Editor.Graph;
using Ceres.Editor.Graph.Flow;
using UEditor = UnityEditor.Editor;

namespace Kurisu.NGDT.Editor
{
    public class NextGenDialogueEditor: UEditor
    {
        internal class DialogueGraphButton : Button
        {
            private const string ButtonText = "Open Dialogue Graph";
            
            public IDialogueGraphContainer Container { get; set; }
            
            public DialogueGraphButton(IDialogueGraphContainer container) 
            {
                style.fontSize = 15;
                style.unityFontStyleAndWeight = FontStyle.Bold;
                style.color = Color.white;
                style.backgroundColor = new StyleColor(new Color(85 / 255f, 205 / 255f, 115 / 255f));
                text = ButtonText;
                Container = container;
                clickable = new Clickable(() => DialogueEditorWindow.Show(Container));
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
        
        protected static readonly string LabelText = $"Next-Gen Dialogue";
    }

    [CustomEditor(typeof(NextGenDialogueComponent))]
    public class NextGenDialogueComponentEditor : NextGenDialogueEditor
    {
        private DialogueGraphButton _dialogueGraphButton;
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
            var component = (NextGenDialogueComponent)Target;
            myInspector.Add(label);
            myInspector.styleSheets.Add(CeresGraphView.GetOrLoadStyleSheet(NextGenDialogueSettings.InspectorStylePath));
            var assetField = new PropertyField(serializedObject.FindProperty("externalAsset"), "External Asset");
            myInspector.Add(assetField);
            
            if(!component.Asset)
            {
                // create instance for edit
                var instance = Target.GetDialogueGraph();
                if (instance.variables.Count(x => x?.IsExposed ?? false) != 0)
                {
                    myInspector.Add(new SharedVariablesFoldout(instance.BlackBoard, () =>
                    {
                        // Not serialize data in playing mode
                        if (Application.isPlaying) return;
                        Target.SetGraphData(instance.GetData());
                        EditorUtility.SetDirty(target);
                    }));
                }
            }
            
            assetField.RegisterValueChangeCallback(evt =>
            {
                var externalAsset = evt.changedProperty.objectReferenceValue as NextGenDialogueGraphAsset;
                _dialogueGraphButton.Container = (bool)externalAsset ? externalAsset : Target;
            });

            _dialogueGraphButton = new DialogueGraphButton((bool)component.Asset ? component.Asset : Target);
            myInspector.Add(_dialogueGraphButton);
            
            var playButton = new DialogueGraphPlayDialogueButton(component);
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
            myInspector.styleSheets.Add(CeresGraphView.GetOrLoadStyleSheet(NextGenDialogueSettings.InspectorStylePath));
            myInspector.Add(new Label("Description"));
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
            var instance = Target.GetDialogueGraph();
            if (instance.variables.Count(x => x?.IsExposed ?? false) != 0)
            {
                myInspector.Add(new SharedVariablesFoldout(instance.BlackBoard, () =>
                {
                    // Not serialize data in playing mode
                    if (Application.isPlaying) return;
                    Target.SetGraphData(instance.GetData());
                    EditorUtility.SetDirty(target);
                }));
            }
            myInspector.Add(new PropertyField(serializedObject.FindProperty("flowGraphAsset")));
            myInspector.Add(new DialogueGraphButton(Target));
            var asset = (NextGenDialogueGraphAsset)Target;
            myInspector.Add(new FlowGraphDebugButton(asset));
            return myInspector;
        }
    }
}
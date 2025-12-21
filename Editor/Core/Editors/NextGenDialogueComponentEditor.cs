using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Ceres.Editor.Graph;
using Ceres.Editor.Graph.Flow;
using Ceres.Graph.Flow;
using UEditor = UnityEditor.Editor;

namespace NextGenDialogue.Graph.Editor
{
    internal static class Styles
    {
        public class DialogueGraphOpenButton : Button
        {
            private const string ButtonText = "Open Dialogue Graph";

            public DialogueGraphOpenButton(IDialogueGraphContainer container) 
            {
                style.fontSize = 15;
                style.unityFontStyleAndWeight = FontStyle.Bold;
                style.color = Color.white;
                style.backgroundColor = new StyleColor(new Color(85 / 255f, 205 / 255f, 115 / 255f));
                text = ButtonText;
                clickable = new Clickable(() => DialogueEditorWindow.Show(container));
            }
        }
        
        public class FlowGraphButton : Button
        {
            private const string ButtonText = "Open Flow Graph";
        
            public FlowGraphButton(IFlowGraphContainer container) : base(() => FlowGraphEditorWindow.Show(container))
            {
                style.fontSize = 15;
                style.unityFontStyleAndWeight = FontStyle.Bold;
                style.color = Color.white;
                style.backgroundColor = new StyleColor(new Color(89 / 255f, 133 / 255f, 141 / 255f));
                text = ButtonText;
                Add(new Image
                {
                    style =
                    {
                        backgroundImage = Resources.Load<Texture2D>("Ceres/editor_icon"),
                        height = 20,
                        width = 20
                    }
                });
                style.height = 25;
            }
        }
        
        public class DialogueGraphPlayButton : Button
        {
            private const string ButtonText = "Play Dialogue";
            
            public DialogueGraphPlayButton(NextGenDialogueComponent component) : base(component.PlayDialogue)
            {
                style.fontSize = 15;
                style.unityFontStyleAndWeight = FontStyle.Bold;
                style.color = Color.white;
                style.backgroundColor = new StyleColor(new Color(85 / 255f, 205 / 255f, 115 / 255f));
                text = ButtonText;
            }
        }

        public const string LabelText = "Next-Gen Dialogue";
    }

    [CustomEditor(typeof(NextGenDialogueComponent), true)]
    public class NextGenDialogueComponentEditor : UEditor
    {
        public NextGenDialogueComponent Target => (NextGenDialogueComponent)target;

        private SerializedProperty _externalAsset;

        public void OnEnable()
        {
            _externalAsset = serializedObject.FindProperty("externalAsset");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var label = new Label(Styles.LabelText)
            {
                style =
                {
                    fontSize = 20,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            var component = Target;
            myInspector.Add(label);
            myInspector.styleSheets.Add(CeresGraphView.GetOrLoadStyleSheet(NextGenDialogueSettings.InspectorStylePath));
            
            var assetField = new PropertyField(_externalAsset, "External Asset");
            assetField.Bind(serializedObject);
            myInspector.Add(assetField);
            
            if (!component.Asset)
            {
                var dialogueBlackboardPanel = new BlackboardInspectorPanel(Target.GetDialogueGraph,
                    () => ((IDialogueGraphContainer)Target).GetDialogueGraphData().saveTimestamp,
                    instance =>
                    {
                        // Do not serialize data in playing mode
                        if (Application.isPlaying) return;

                        Target.SetGraphData(((DialogueGraph)instance).GetData());
                        EditorUtility.SetDirty(target);
                    })
                {
                    Subtitle = "(Dialogue Graph)"
                };
                myInspector.Add(dialogueBlackboardPanel);
            }
            
            var graphButton = new Styles.DialogueGraphOpenButton(component);
            if (component.Asset)
            {
                graphButton.SetEnabled(false);
            }
            myInspector.Add(graphButton);

            var flowBlackboardPanel = new BlackboardInspectorPanel(
                () => Target.GetFlowGraph(),
                () => ((IFlowGraphContainer)Target).GetFlowGraphData().saveTimestamp,
                instance =>
                {
                    // Do not serialize data in playing mode
                    if (Application.isPlaying) return;

                    var graphData = ((IFlowGraphContainer)Target).GetFlowGraphData().CloneT<FlowGraphData>();
                    graphData.variableData = instance.variables.Where(variable => variable is not LocalFunction)
                        .Select(variable => variable.GetSerializedData())
                        .ToArray();
                    Target.SetGraphData(graphData);
                    EditorUtility.SetDirty(target);
                })
            {
                Subtitle = "(Flow Graph)"
            };
            myInspector.Add(flowBlackboardPanel);

            var flowButton = new Styles.FlowGraphButton(component);
            if (component.Asset)
            {
                flowButton.SetEnabled(false);
            }
            myInspector.Add(flowButton);
            
            var playButton = new Styles.DialogueGraphPlayButton(component);
            playButton.SetEnabled(Application.isPlaying);
            myInspector.Add(playButton);
            return myInspector;
        }
    }
}
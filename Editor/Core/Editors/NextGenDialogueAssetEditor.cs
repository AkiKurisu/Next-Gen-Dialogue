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
    [CustomEditor(typeof(NextGenDialogueGraphAsset), true)]
    public class NextGenDialogueAssetEditor : UEditor
    {
        public NextGenDialogueGraphAsset Target => (NextGenDialogueGraphAsset)target;

        private SerializedProperty _flowGraphAsset;

        private void OnEnable()
        {
            _flowGraphAsset = serializedObject.FindProperty("flowGraphAsset");
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
            
            myInspector.Add(new Styles.DialogueGraphOpenButton(Target));
            
            var propertyField = new PropertyField(_flowGraphAsset);
            propertyField.Bind(serializedObject);
            myInspector.Add(propertyField);
            
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
            if (_flowGraphAsset.objectReferenceValue)
            {
                flowBlackboardPanel.SetEnabled(false);
            }
            myInspector.Add(flowBlackboardPanel);

            var graphButton = new Styles.FlowGraphButton(Target);
            if (_flowGraphAsset.objectReferenceValue)
            {
                graphButton.SetEnabled(false);
            }
            myInspector.Add(graphButton);
            return myInspector;
        }
    }
}
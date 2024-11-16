using System.Linq;
using Ceres;
using Ceres.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class DialogueBlackboard: CeresBlackboard
    {
        public DialogueBlackboard(IVariableSource variableSource, GraphView graphView) : base(variableSource, graphView)
        {
        }

        protected override BlackboardRow CreateBlackboardPropertyRow(SharedVariable variable, BlackboardField blackboardField, VisualElement valueField)
        {
            var propertyView = new VisualElement();
            if (!AlwaysExposed && variable is not PieceID)
            {
                var toggle = new Toggle("Exposed")
                {
                    value = variable.IsExposed
                };
                if (Application.isPlaying)
                {
                    toggle.SetEnabled(false);
                }
                else
                {
                    toggle.RegisterValueChangedCallback(x =>
                    {
                        var index = sharedVariables.FindIndex(x => x.Name == variable.Name);
                        sharedVariables[index].IsExposed = x.newValue;
                        NotifyVariableChanged(variable, VariableChangeType.ValueChange);
                    });
                }
                propertyView.Add(toggle);
            }
            if (variable is PieceID)
            {
                blackboardField.RegisterCallback<ClickEvent>((evt) => FindRelatedPiece(variable));
            }
            else
            {
                propertyView.Add(valueField);
            }
            if (variable is SharedObject sharedObject)
            {
                propertyView.Add(GetConstraintField(sharedObject, (ObjectField)valueField));
            }
            var sa = new BlackboardRow(blackboardField, propertyView);
            if (variable is PieceID)
            {
                sa.Q<Button>("expandButton").RemoveFromHierarchy();
            }
            sa.AddManipulator(new ContextualMenuManipulator((evt) => BuildBlackboardMenu(evt, variable)));
            return sa;
        }
        private void FindRelatedPiece(SharedVariable variable)
        {
            var piece = graphView.nodes.OfType<PieceContainer>().FirstOrDefault(x => x.GetPieceID() == variable.Name);
            if (piece != null)
            {
                graphView.AddToSelection(piece);
            }
        }
    }
}
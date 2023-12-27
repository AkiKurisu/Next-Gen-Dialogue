using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueTreeViewExtension
    {
        public static T GetSharedVariableValue<T>(this IDialogueTreeView treeView, SharedVariable<T> variable)
        {
            if (variable.IsShared)
            {
                if (!treeView.TryGetProperty(variable.Name, out SharedVariable<T> mapContent)) return variable.Value;
                return mapContent.Value;
            }
            else
            {
                return variable.Value;
            }
        }
        public static bool TryGetProperty<T>(this IDialogueTreeView treeView, string name, out T variable) where T : SharedVariable
        {
            variable = (T)treeView.SharedVariables.Where(x => x is T && x.Name.Equals(name)).FirstOrDefault();
            return variable != null;
        }
        public static PieceContainer FindPiece(this IDialogueTreeView treeView, string pieceID)
        {
            return treeView.View.nodes.OfType<PieceContainer>().FirstOrDefault(x => x.GetPieceID() == pieceID);
        }
        public static List<T> CollectNodes<T>(this IDialogueTreeView treeView) where T : Node
        {
            return treeView.View.nodes.OfType<T>().ToList();
        }
    }
}

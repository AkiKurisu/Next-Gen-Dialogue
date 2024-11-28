using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public class NodeSettingsView : VisualElement
    {
        private readonly VisualElement m_ContentContainer;
        public Node ParentNode { get; private set; }
        public NodeSettingsView(Node parentNode)
        {
            ParentNode = parentNode;
            pickingMode = PickingMode.Ignore;
            styleSheets.Add(Resources.Load<StyleSheet>("NGDT/NodeSettings"));
            var uxml = Resources.Load<VisualTreeAsset>("NGDT/UXML/NodeSettings");
            uxml.CloneTree(this);
            // Get the element we want to use as content container
            m_ContentContainer = this.Q("contentContainer");
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            evt.StopPropagation();
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            evt.StopPropagation();
        }

        public override VisualElement contentContainer
        {
            get { return m_ContentContainer; }
        }
    }
}
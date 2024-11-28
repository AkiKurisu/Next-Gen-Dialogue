using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class DialogueTreeLayoutConvertor : INodeForLayoutConvertor
    {
        private class VirtualTreeView : ILayoutTreeNode
        {
            private readonly ILayoutTreeNode node;
            private readonly VisualElement view;
            public VirtualTreeView(VisualElement view, ILayoutTreeNode node)
            {
                this.node = node;
                this.view = view;
            }
            public VisualElement View => view;

            public IReadOnlyList<ILayoutTreeNode> GetLayoutTreeChildren()
            {
                return new List<ILayoutTreeNode>() { node };
            }
        }
        public float SiblingDistance { get; } = 50;
        private NodeAutoLayoutHelper.TreeNode m_LayoutRootNode;
        public NodeAutoLayoutHelper.TreeNode LayoutRootNode => m_LayoutRootNode;
        private readonly ILayoutTreeNode m_PrimRootNode;
        private const NodeAutoLayoutHelper.CalculateMode CalculateMode = NodeAutoLayoutHelper.CalculateMode.Horizontal | NodeAutoLayoutHelper.CalculateMode.Positive;
        public DialogueTreeLayoutConvertor(VisualElement view, ILayoutTreeNode primRootNode)
        {
            m_PrimRootNode = new VirtualTreeView(view, primRootNode);
        }
        public NodeAutoLayoutHelper.TreeNode PrimNode2LayoutNode()
        {
            if (m_PrimRootNode.View.layout.width == float.NaN)
            {
                return null;
            }

            m_LayoutRootNode =
                new NodeAutoLayoutHelper.TreeNode(m_PrimRootNode.View.layout.height + SiblingDistance,
                    m_PrimRootNode.View.layout.width,
                    m_PrimRootNode.View.layout.y,
                    CalculateMode);

            Convert2LayoutNode(m_PrimRootNode,
                m_LayoutRootNode, m_PrimRootNode.View.layout.y + m_PrimRootNode.View.layout.width,
                CalculateMode);
            return m_LayoutRootNode;
        }

        private void Convert2LayoutNode(ILayoutTreeNode rootPrimNode,
            NodeAutoLayoutHelper.TreeNode rootLayoutNode, float lastHeightPoint,
            NodeAutoLayoutHelper.CalculateMode calculateMode)
        {
            foreach (var childNode in rootPrimNode.GetLayoutTreeChildren())
            {
                NodeAutoLayoutHelper.TreeNode childLayoutNode =
                    new(childNode.View.layout.height + SiblingDistance, childNode.View.layout.width,
                        lastHeightPoint + SiblingDistance,
                        calculateMode);
                rootLayoutNode.AddChild(childLayoutNode);
                Convert2LayoutNode(childNode, childLayoutNode,
                    lastHeightPoint + SiblingDistance + childNode.View.layout.width, calculateMode);
            }
        }

        public void LayoutNode2PrimNode()
        {
            var rootNode = m_PrimRootNode.GetLayoutTreeChildren()[0];
            var graphElement = rootNode.View as GraphElement;
            var offSet = graphElement.contentRect.position - m_LayoutRootNode.children[0].GetPos()
            + graphElement.GetPosition().position;
            Convert2PrimNode(rootNode, m_LayoutRootNode.children[0], offSet);
        }

        private void Convert2PrimNode(
            ILayoutTreeNode rootPrimNode,
            NodeAutoLayoutHelper.TreeNode rootLayoutNode,
            Vector2 offSet
        )
        {
            var children = rootPrimNode.GetLayoutTreeChildren();
            for (int i = 0; i < rootLayoutNode.children.Count; i++)
            {
                Convert2PrimNode(children[i], rootLayoutNode.children[i], offSet);
                Vector2 calculateResult = rootLayoutNode.children[i].GetPos();
                if (children[i].View is GraphElement graphElement)
                    graphElement.SetPosition(new Rect(calculateResult + offSet, graphElement.contentRect.size));
            }
        }
    }
}

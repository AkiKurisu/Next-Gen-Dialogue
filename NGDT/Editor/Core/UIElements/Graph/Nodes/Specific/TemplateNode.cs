using System;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(TemplateModule))]
    public class TemplateNode : EditorModuleNode
    {
        public TemplateNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            var label = new Label("Template")
            {
                style =
                {
                    fontSize = 14f,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new Color(255 / 255f, 239 / 255f, 0f, 0.91f)
                }
            };
            mainContainer.Add(label);
        }
    }
}
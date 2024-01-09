using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [Ordered]
    public class TemplateResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new TemplateNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(TemplateModule);
        private class TemplateNode : EditorModuleNode
        {
            public TemplateNode()
            {
                var label = new Label("Template");
                label.style.fontSize = 14f;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.color = new Color(255 / 255f, 239 / 255f, 0f, 0.91f);
                mainContainer.Add(label);
            }
        }
    }
}

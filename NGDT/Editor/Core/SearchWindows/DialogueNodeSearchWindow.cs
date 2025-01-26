using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using Ceres.Utilities;
using Ceres.Editor.Graph;
namespace Kurisu.NGDT.Editor
{
    public class DialogueNodeSearchWindow : CeresNodeSearchWindow
    {
        private Texture2D _indentationIcon;

        private DialogueGraphView DialogueView => (DialogueGraphView)GraphView;
        
        protected override void OnInitialize()
        {
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }

        private static readonly Type[] FilteredTypes = {
            typeof(Container),
            typeof(Action),
            typeof(Conditional),
            typeof(Composite),
            typeof(Decorator)
        };
        
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
            };
            
            var (groups, nodeTypes) = SearchTypes(FilteredTypes, Context);
            var builder = new CeresNodeSearchEntryBuilder(_indentationIcon, Context.AllowGeneric, Context.ParameterType);
            foreach (var filteredType in FilteredTypes)
            {
                builder.AddEntry(new SearchTreeGroupEntry(new GUIContent($"Select {filteredType.Name}"), 1));
                var subGroups = groups.SelectSubclass(filteredType);
                foreach (var subGroup in subGroups)
                {
                    builder.AddAllEntries(subGroup, 2);
                }
                var left = nodeTypes.Where(x => x.IsSubclassOf(filteredType));
                foreach (var type in left)
                {
                    builder.AddEntry(type, 2);
                }
            }
            entries.AddRange(builder.GetEntries());
            entries.Add(new SearchTreeEntry(new GUIContent("Create Group Block", _indentationIcon))
            { 
                level = 1, 
                userData = new CeresNodeSearchEntryData()
                {
                    NodeType = typeof(DialogueNodeGroup)
                } 
            });
            return entries;
        }
        public override bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Rect newRect = new(GraphView.Screen2GraphPosition(context.screenMousePosition), new Vector2(100, 100));
            var entryData = (CeresNodeSearchEntryData)searchTreeEntry.userData;
            var type = entryData.NodeType;
            if (type == typeof(DialogueNodeGroup))
            {
                GraphView.NodeGroupHandler.CreateGroup(newRect);
                return true;
            }
            if (type.IsSubclassOf(typeof(Container)))
            {
                var templateNode = DialogueView.CollectNodes<ContainerNode>()
                                                .Where(x => x.GetBehavior() == type)
                                                .FirstOrDefault(x => x.TryGetModuleNode<TemplateModule>(out _));
                if (templateNode != null)
                {
                    // Use template instead
                    var instance = (ContainerNode)DialogueView.DuplicateNode(templateNode);
                    instance!.SetPosition(newRect);
                    instance.RemoveModule<TemplateModule>();
                    return true;
                }
            }
            var node = NodeViewFactory.Get().CreateInstance(type, DialogueView);
            if (node is PieceContainer pieceContainer)
            {
                pieceContainer.GenerateNewPieceID();
            }
            DialogueView.AddNodeView(node, newRect);
            return true;
        }
    }
}

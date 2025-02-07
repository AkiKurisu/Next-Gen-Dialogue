using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using Ceres.Utilities;
using Ceres.Editor.Graph;
namespace NextGenDialogue.Graph.Editor
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
            typeof(ContainerNode),
            typeof(ActionNode),
            typeof(CompositeNode)
        };
        
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"))
            };
            
            var (groups, nodeTypes) = SearchTypes(FilteredTypes, Context);
            var builder = new CeresNodeSearchEntryBuilder(_indentationIcon, Context.AllowGeneric, Context.ParameterType);
            
            foreach (var filteredType in FilteredTypes)
            {
                builder.AddEntry(new SearchTreeGroupEntry(new GUIContent($"Select {filteredType.Name.Replace("Node", string.Empty)}"), 1));
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
            return entries;
        }
        public override bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Rect newRect = new(GraphView.Screen2GraphPosition(context.screenMousePosition), new Vector2(100, 100));
            var entryData = (CeresNodeSearchEntryData)searchTreeEntry.userData;
            var type = entryData.NodeType;
            var node = NodeViewFactory.Get().CreateInstance(type, DialogueView);
            if (node is PieceContainerView pieceContainer)
            {
                pieceContainer.GenerateNewPieceID();
                pieceContainer.AddModuleNode(new ContentModule());
            }
            else if (node is OptionContainerView optionContainer)
            {
                optionContainer.AddModuleNode(new ContentModule());
            }
            DialogueView.AddNodeView(node, newRect);
            return true;
        }
    }
}

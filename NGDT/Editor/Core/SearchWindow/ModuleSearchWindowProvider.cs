using System;
using System.Collections.Generic;
using System.Linq;
using Ceres;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class ModuleSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private ContainerNode _node;
        
        private Type _containerType;
        
        private Texture2D _indentationIcon;

        private NodeSearchContext _context;
        
        private DialogueTreeView _treeView;
        
        private IEnumerable<Type> _exceptTypes;
        
        public void Init(ContainerNode node, DialogueTreeView treeView, NodeSearchContext context, IEnumerable<Type> exceptTypes)
        {
            _exceptTypes = exceptTypes;
            _treeView = treeView;
            _node = node;
            _context = context;
            _containerType = node.GetBehavior();
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }
        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select {nameof(Module)}"), 0));
            List<Type> subClasses = SubClassSearchUtility.FindSubClassTypes(typeof(Module)).Except(_exceptTypes)
                                .Where(x =>
                                {
                                    var validTypes = (ModuleOfAttribute[])x.GetCustomAttributes(typeof(ModuleOfAttribute), true);
                                    return validTypes.Length != 0 && validTypes.Any(attribute => attribute.ContainerType == _containerType);
                                })
                                .ToList();
            var list = subClasses.GroupsByNodeGroup().ToList(); ;
            var nodeTypes = subClasses.Except(list.SelectMany(x => x)).ToList();
            var groups = list.SelectGroup(_context.ShowGroups).ExceptGroup(_context.HideGroups).ToList();
            var builder = new CeresNodeSearchEntryBuilder(_indentationIcon);
            foreach (var group in groups)
            {
                builder.AddAllEntries(group, 1);
            }
            foreach (var type in nodeTypes)
            {
                builder.AddEntry(type, 1);
            }
            entries.AddRange(builder.GetEntries());
            return entries;
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var entryData = (CeresNodeSearchEntryData)searchTreeEntry.userData;
            var type = entryData.NodeType;
            var moduleNode = DialogueNodeFactory.Get().Create(type, _treeView) as ModuleNode;
            _node.AddElement(moduleNode);
            moduleNode!.OnSelect = _treeView.OnSelectNode;
            return true;
        }
    }
}
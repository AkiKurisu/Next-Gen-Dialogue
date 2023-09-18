using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class ModuleSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private ContainerNode node;
        private Type ContainerType;
        private Texture2D _indentationIcon;
        private readonly NodeResolverFactory nodeResolver = NodeResolverFactory.Instance;
        private string[] showGroups;
        private string[] notShowGroups;
        private IDialogueTreeView treeView;
        private IEnumerable<Type> exceptTypes;
        public void Init(ContainerNode node, IDialogueTreeView treeView, (string[], string[]) mask, IEnumerable<Type> exceptTypes)
        {
            this.exceptTypes = exceptTypes;
            this.treeView = treeView;
            this.node = node;
            ContainerType = node.GetBehavior();
            showGroups = mask.Item1;
            notShowGroups = mask.Item2;
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }
        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            Dictionary<string, List<Type>> attributeDict = new();

            entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select {typeof(Module).Name}"), 0));
            List<Type> nodeTypes = SubclassSearchUtility.FindSubClassTypes(typeof(Module));
            nodeTypes = nodeTypes.Except(exceptTypes)
            .Where(x =>
            {
                var validTypes = x.GetCustomAttributes(typeof(ModuleOfAttribute), true);
                return validTypes.Length != 0 && validTypes.Any(x => ((ModuleOfAttribute)x).ContainerType == ContainerType);
            })
            .ToList();
            var groups = nodeTypes.GroupsByAkiGroup();
            nodeTypes = nodeTypes.Except(groups.SelectMany(x => x)).ToList();
            groups = groups.SelectGroup(showGroups).ExceptGroup(notShowGroups);
            foreach (var group in groups)
            {
                entries.AddAllEntries(group, _indentationIcon, 1);
            }
            foreach (Type type in nodeTypes)
            {
                entries.AddEntry(type, 1, _indentationIcon);
            }
            return entries;
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var type = searchTreeEntry.userData as Type;
            var moduleNode = nodeResolver.Create(type, treeView) as ModuleNode;
            node.AddElement(moduleNode);
            moduleNode.OnSelectAction = treeView.OnSelectAction;
            return true;
        }
    }
}
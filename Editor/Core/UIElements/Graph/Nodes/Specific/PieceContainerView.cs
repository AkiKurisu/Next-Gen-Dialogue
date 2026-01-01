using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    [CustomNodeView(typeof(Piece))]
    public sealed class PieceContainerView : ContainerNodeView, IContainChildNode
    {
        public string GetPieceID()
        {
            return mainContainer.Q<PieceIDField>().value.Name;
        }
        
        public Piece GetPiece()
        {
            return (Piece)NodeInstance;
        }

        protected override Port.Capacity PortCapacity => Port.Capacity.Multi;

        protected override Type ParentPortType => typeof(PiecePort);

        protected override Color PortColor => new(60 / 255f, 140 / 255f, 171 / 255f, 0.91f);
        
        public PieceContainerView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            name = nameof(PieceContainerView);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            GraphView?.Blackboard?.RemoveVariable(mainContainer.Q<PieceIDField>().BindVariable, true);
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            base.OnSeparatorContextualMenuEvent(evt, separatorIndex);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Option", a =>
            {
                // Create option
                var option = (OptionContainerView)GraphView.CreateNextContainer(this);
                option.AddModuleNode(new ContentModule());
                // Link to bridge node
                this.ConnectContainerNodes(option);
            }));
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Edit PieceID", a =>
            {
                GraphView.Blackboard.EditVariable(GetPieceID());
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Auto Layout", a =>
            {
                new DialogueTreeLayoutConvertor(GraphView, this).Layout();
            }));
            
            // Preview from here (editor simulator)
            if (!Application.isPlaying)
            {
                evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Preview from here", a =>
                {
                    if (GraphView.EditorWindow is DialogueEditorWindow dialogueEditorWindow)
                    {
                        dialogueEditorWindow.StartSimulatorFromPiece(this);
                    }
                }));
            }
            
            if (Application.isPlaying)
            {
                evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Jump to this Piece", a =>
                {
                    DialogueSystem.Get().PlayDialoguePiece(GetPiece().CastPiece().ID);
                },
                _ =>
                {
                    var ds = DialogueSystem.Get();
                    if (!ds.IsPlaying) return DropdownMenuAction.Status.Disabled;
                    // Whether is the container of this piece
                    var piece = GetPiece().CastPiece();
                    if (ds.GetPlayingDialogue()?.GetPiece(piece.ID) != piece) return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                }));
            }
            base.BuildContextualMenu(evt);
        }
        
        public void AddChildElement(IDialogueNodeView node)
        {
            var bridge = new OptionBridgeView("Option", PortColor);
            AddElement(bridge);
            var edge = PortHelper.ConnectPorts(bridge.Child, node.Parent);
            GraphView.Add(edge);
        }
        
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is not ModuleNodeView moduleNode) return element is OptionBridgeView or ParentBridgeView;
            var nodeType = moduleNode.NodeType;
            var define = nodeType.GetCustomAttributes<ModuleOfAttribute>().FirstOrDefault(x => x.ContainerType == typeof(Piece));
            if (define == null) return false;
            if (define.AllowMulti) return true;
            return !TryGetModuleNode(nodeType, out _);
        }
        
        public void GenerateNewPieceID()
        {
            var variable = new PieceID { Name = "New Piece" };
            GraphView.Blackboard.AddVariable(variable, false);
            mainContainer.Q<PieceIDField>().value = new PieceID { Name = variable.Name };
        }
        
        protected override IEnumerable<Type> GetExceptModuleTypes()
        {
            return contentContainer.Query<ModuleNodeView>().ToList().Select(x => x.NodeType).Where(x =>
            {
                var define = x.GetCustomAttributes<ModuleOfAttribute>()
                                            .First(attribute => attribute.ContainerType == typeof(Piece));
                return !define.AllowMulti;
            });
        }

        public OptionContainerView[] GetConnectedOptionContainers()
        {
            var bridges = contentContainer.Query<ChildBridgeView>().ToList();
            return bridges.Select(bridge => PortHelper.FindChildNode(bridge.Child)).OfType<OptionContainerView>().ToArray();
        }
    }
   
}

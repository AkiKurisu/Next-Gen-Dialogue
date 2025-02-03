using System;
using System.IO;
using System.Threading.Tasks;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(OobaboogaSessionModule))]
    public class OobaboogaSessionNodeView : EditorModuleNodeView
    {
        private bool IsPending { get; set; }
        
        public OobaboogaSessionNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            var label = new Label("Right-click and load Oobabooga Session")
            {
                style =
                {
                    fontSize = 14f
                }
            };
            mainContainer.Add(label);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            //The custom contextual menu builder will only activate when this editor node is attached
            Graph.ContextualMenuRegistry.Register<OobaboogaSessionNodeView>(new SessionContextualMenuBuilder(this));
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            //Do not forget to unregister after detach
            Graph.ContextualMenuRegistry.UnRegister<OobaboogaSessionNodeView>();
        }

        private async void LoadSession(Vector2 mousePosition)
        {
            string path = EditorUtility.OpenFilePanel("Select Oobabooga Session", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path)) return;
            IsPending = true;
            try
            {
                var session = JsonConvert.DeserializeObject<OobaboogaSession>(await File.ReadAllTextAsync(path));
                var internalData = session.history.internalData;
                //Add first piece
                var position = Graph.contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
                var firstPiece = (PieceContainerView)Graph.CreateNode(new Piece(), position);
                firstPiece.GenerateNewPieceID();
                firstPiece.AddModuleNode(new ContentModule(internalData[0][1]));
                ContainerNodeView last = firstPiece;
                for (int i = 1; i < internalData.Length; ++i)
                {
                    // Create option
                    var node = Graph.CreateNextContainer(last);
                    // Link to piece
                    Graph.ConnectContainerNodes(last, node);
                    node.AddModuleNode(new ContentModule(internalData[i][0]));
                    last = node;
                    // Create next container
                    node = Graph.CreateNextContainer(last);
                    // Link to option
                    Graph.ConnectContainerNodes(last, node);
                    node.AddModuleNode(new ContentModule(internalData[i][1]));
                    last = node;
                }
                //Add context copied from session
                GetFirstAncestorOfType<DialogueContainerView>()?
                    .AddModuleNode(new SystemPromptModule(session.context));
                await Task.Delay(2);
                //Auto layout
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(Graph, firstPiece));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                IsPending = false;
            }
        }
        
        private class SessionContextualMenuBuilder : ContextualMenuBuilder
        {
            public SessionContextualMenuBuilder(OobaboogaSessionNodeView nodeView) :
            base(
                ContextualMenuType.Graph,
                _ => !nodeView.IsPending,
                (evt) =>
                {
                    evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Load Oobabooga Session", _ =>
                        {
                            nodeView.LoadSession(evt.mousePosition);
                        }));
                })
            { }
        }
    }

}
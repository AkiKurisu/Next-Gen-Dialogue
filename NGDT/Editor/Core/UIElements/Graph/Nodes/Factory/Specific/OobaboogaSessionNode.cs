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
    public class OobaboogaSessionNode : EditorModuleNode
    {
        public bool IsPending { get; private set; }
        public OobaboogaSessionNode()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            var label = new Label("Right-click and load Oobabooga Session");
            label.style.fontSize = 14f;
            mainContainer.Add(label);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            //The custom contextual menu builder will only activate when this editor node is attached
            GraphView.ContextualMenuRegistry.Register<OobaboogaSessionNode>(new SessionContextualMenuBuilder(this));
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            //Do not forget to unregister after detach
            GraphView.ContextualMenuRegistry.UnRegister<OobaboogaSessionNode>();
        }
        internal async void LoadSession(Vector2 mousePosition)
        {
            string path = EditorUtility.OpenFilePanel("Select Oobabooga Session", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path)) return;
            IsPending = true;
            try
            {
                var session = JsonConvert.DeserializeObject<OobaboogaSession>(await File.ReadAllTextAsync(path));
                var internalData = session.history.internalData;
                //Add first piece
                var position = GraphView.contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
                var firstPiece = GraphView.CreateNode(new Piece(), position) as PieceContainer;
                firstPiece.GenerateNewPieceID();
                firstPiece.AddModuleNode(new ContentModule(internalData[0][1]));
                ContainerNode last = firstPiece;
                for (int i = 1; i < internalData.Length; ++i)
                {
                    // Create option
                    var node = GraphView.CreateNextContainer(last);
                    // Link to piece
                    GraphView.ConnectContainerNodes(last, node);
                    node.AddModuleNode(new ContentModule(internalData[i][0]));
                    last = node;
                    // Create next container
                    node = GraphView.CreateNextContainer(last);
                    // Link to option
                    GraphView.ConnectContainerNodes(last, node);
                    node.AddModuleNode(new ContentModule(internalData[i][1]));
                    last = node;
                }
                //Add context copied from session
                GetFirstAncestorOfType<DialogueContainer>()?
                    .AddModuleNode(new SystemPromptModule(session.context));
                await Task.Delay(2);
                //Auto layout
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(GraphView, firstPiece));
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
            public SessionContextualMenuBuilder(OobaboogaSessionNode node) :
            base(
                ContextualMenuType.Graph,
                (x) => !node.IsPending,
                (evt) =>
                {
                    var target = evt.target;
                    evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Load Oobabooga Session", (a) =>
                        {
                            node.LoadSession(evt.mousePosition);
                        }));
                })
            { }
        }
    }

}
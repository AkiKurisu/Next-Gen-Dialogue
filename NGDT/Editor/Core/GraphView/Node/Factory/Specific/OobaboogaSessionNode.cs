using System;
using System.IO;
using System.Threading.Tasks;
using Ceres.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeEditor(typeof(OobaboogaSessionModule))]
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
            MapTreeView.ContextualMenuRegistry.Register<OobaboogaSessionNode>(new SessionContextualMenuBuilder(this));
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            //Do not forget to unregister after detach
            MapTreeView.ContextualMenuRegistry.UnRegister<OobaboogaSessionNode>();
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
                var position = MapTreeView.contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
                var firstPiece = MapTreeView.CreateNode(new Piece(), position) as PieceContainer;
                firstPiece.GenerateNewPieceID();
                firstPiece.AddModuleNode(new ContentModule(internalData[0][1]));
                ContainerNode last = firstPiece;
                for (int i = 1; i < internalData.Length; ++i)
                {
                    // Create option
                    var node = MapTreeView.CreateNextContainer(last);
                    // Link to piece
                    MapTreeView.ConnectContainerNodes(last, node);
                    node.AddModuleNode(new ContentModule(internalData[i][0]));
                    last = node;
                    // Create next container
                    node = MapTreeView.CreateNextContainer(last);
                    // Link to option
                    MapTreeView.ConnectContainerNodes(last, node);
                    node.AddModuleNode(new ContentModule(internalData[i][1]));
                    last = node;
                }
                //Add context copied from session
                GetFirstAncestorOfType<DialogueContainer>()?
                    .AddModuleNode(new SystemPromptModule(session.context));
                await Task.Delay(2);
                //Auto layout
                NodeAutoLayoutHelper.Layout(new DialogueTreeLayoutConvertor(MapTreeView, firstPiece));
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
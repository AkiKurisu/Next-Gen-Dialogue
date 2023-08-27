using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using System.Linq;
using System;
namespace Kurisu.NGDT.Editor
{
    [CustomEditor(typeof(NextGenDialogueTree))]
    public class NextGenDialogueTreeEditor : UnityEditor.Editor
    {
        private const string LabelText = "Next-Gen DialogueTree <size=12>Version1.0</size>";
        private const string ButtonText = "Edit DialogueTree";
        private const string DebugText = "Open DialogueTree (Runtime)";
        private VisualElement myInspector;
        private readonly FieldResolverFactory factory = new();
        public override VisualElement CreateInspectorGUI()
        {
            myInspector = new VisualElement();
            var bt = target as NextGenDialogueTree;
            var label = new Label(LabelText);
            label.style.fontSize = 20;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSetting.GetInspectorStyle());
            var field = new PropertyField(serializedObject.FindProperty("externalDialogueTree"), "External Dialogue Tree");
            myInspector.Add(field);
            DialogueEditorUtility.DrawSharedVariable(myInspector, bt, factory, target, this);
            var button = DialogueEditorUtility.GetButton(() => DialogueEditorWindow.Show(bt));
            if (!Application.isPlaying)
            {
                button.style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
                button.text = ButtonText;
            }
            else
            {
                button.text = DebugText;
                button.style.backgroundColor = new StyleColor(new Color(253 / 255f, 163 / 255f, 255 / 255f));
            }
            myInspector.Add(button);
            var playButton = DialogueEditorUtility.GetButton(() => bt.PlayDialogue());
            playButton.style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
            playButton.text = "Play Dialogue";
            playButton.SetEnabled(Application.isPlaying);
            myInspector.Add(playButton);
            return myInspector;
        }
    }
    [CustomEditor(typeof(NextGenDialogueTreeSO))]
    public class NextGenDialogueTreeSOEditor : UnityEditor.Editor
    {
        private const string LabelText = "Next-Gen DialogueTreeSO <size=12>Version1.0</size>";
        private const string ButtonText = "Edit DialogueTreeSO";
        private VisualElement myInspector;
        private readonly FieldResolverFactory factory = new();
        public override VisualElement CreateInspectorGUI()
        {
            myInspector = new VisualElement();
            var bt = target as IDialogueTree;
            var label = new Label(LabelText);
            label.style.fontSize = 20;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            myInspector.Add(label);
            myInspector.styleSheets.Add(NextGenDialogueSetting.GetInspectorStyle());
            myInspector.Add(new Label("Editor Description"));
            var description = new PropertyField(serializedObject.FindProperty("Description"), string.Empty);
            myInspector.Add(description);
            DialogueEditorUtility.DrawSharedVariable(myInspector, bt, factory, target, this);
            if (!Application.isPlaying)
            {
                var button = DialogueEditorUtility.GetButton(() => DialogueEditorWindow.Show(bt));
                button.style.backgroundColor = new StyleColor(new Color(140 / 255f, 160 / 255f, 250 / 255f));
                button.text = ButtonText;
                myInspector.Add(button);
            }
            return myInspector;
        }
    }
    internal class DialogueEditorUtility
    {
        internal static Button GetButton(System.Action clickEvent)
        {
            var button = new Button(clickEvent);
            button.style.fontSize = 15;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.color = Color.white;
            return button;
        }
        internal static void DrawSharedVariable(VisualElement parent, IDialogueTree bt, FieldResolverFactory factory, UnityEngine.Object target, UnityEditor.Editor editor)
        {
            int count = bt.SharedVariables.Count(x => x is not PieceID);
            if (count == 0) return;
            var foldout = new Foldout
            {
                value = false,
                text = "SharedVariables"
            };
            foreach (var variable in bt.SharedVariables)
            {
                if (variable is PieceID) continue;
                var grid = new Foldout
                {
                    text = $"{variable.GetType().Name}  :  {variable.Name}",
                    value = false
                };
                var content = new VisualElement();
                content.style.flexDirection = FlexDirection.Row;
                content.style.justifyContent = Justify.SpaceBetween;
                var valueField = factory.Create(variable.GetType().GetField("value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                                        .GetEditorField(bt.SharedVariables, variable);
                if (valueField is TextField field)
                {
                    field.multiline = true;
                    field.style.maxWidth = 250;
                    field.style.whiteSpace = WhiteSpace.Normal;
                }
                valueField.style.width = Length.Percent(70f);
                content.Add(valueField);
                if (variable is SharedObject sharedObject)
                {
                    var objectField = valueField as ObjectField;
                    try
                    {
                        objectField.objectType = Type.GetType(sharedObject.ConstraintTypeAQM, true);
                        grid.text = $"{variable.GetType().Name} ({objectField.objectType.Name})  :  {variable.Name}";
                    }
                    catch
                    {
                        objectField.objectType = typeof(UnityEngine.Object);
                    }
                }
                valueField.RegisterCallback<InputEvent>((e) =>
                {
                    EditorUtility.SetDirty(target);
                    EditorUtility.SetDirty(editor);
                    AssetDatabase.SaveAssets();
                });
                var deleteButton = new Button(() =>
                {
                    bt.SharedVariables.Remove(variable);
                    foldout.Remove(grid);
                    if (bt.SharedVariables.Count(x => x is not PieceID) == 0)
                    {
                        parent.Remove(foldout);
                    }
                    EditorUtility.SetDirty(target);
                    EditorUtility.SetDirty(editor);
                    AssetDatabase.SaveAssets();
                })
                {
                    text = "Delate"
                };
                deleteButton.style.width = Length.Percent(20f);
                deleteButton.style.height = 25;
                content.Add(deleteButton);
                grid.Add(content);
                foldout.Add(grid);
            }
            parent.Add(foldout);
        }
    }
}
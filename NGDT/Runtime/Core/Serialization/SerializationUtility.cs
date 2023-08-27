#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
#endif
using UnityEngine;
namespace Kurisu.NGDT
{
    public class SerializationUtility
    {
        public static string SerializeTree(IDialogueTree behaviorTree, bool indented = false, bool serializeEditorData = false)
        {
            return TemplateToIL(TreeToTemplate(behaviorTree), indented, serializeEditorData);
        }
        public static DialogueTreeTemplate DeserializeTree(string serializedData)
        {
            return JsonUtility.FromJson<DialogueTreeTemplate>(serializedData);
        }
        public static DialogueTreeTemplate TreeToTemplate(IDialogueTree behaviorTree)
        {
            var template = new DialogueTreeTemplate(behaviorTree);
            return template;
        }
        public static string TemplateToIL(DialogueTreeTemplate template, bool indented = false, bool serializeEditorData = false)
        {
            var json = JsonUtility.ToJson(template);
#if UNITY_EDITOR
            //Remove editor only fields in behaviorTree manually
            JObject obj = JObject.Parse(json);
            foreach (JProperty prop in obj.Descendants().OfType<JProperty>().ToList())
            {
                if (!serializeEditorData)
                    if (prop.Name == "graphPosition" || prop.Name == "description" || prop.Name == "guid")
                    {
                        prop.Remove();
                    }
                if (prop.Name == "instanceID")
                {
                    string propertyName = prop.Name;
                    if (prop.Parent?.Parent != null) propertyName = (prop.Parent.Parent as JProperty)?.Name ?? prop.Name;
                    Debug.LogWarning($"<color=#fcbe03>{template.TemplateName}</color> :  Can't serialize UnityEngine.Object field for {propertyName}, desearialization may be failed !");
                }
            }
            return obj.ToString(indented ? Formatting.Indented : Formatting.None);
#else
            //Don't need remove in build version as those fields won't be serialized
            return json;
#endif
        }
    }
}

using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace NextGenDialogue.Graph.Editor
{
    public class PromptTemplate
    {
        private readonly string templateText;
        public PromptTemplate(string path)
        {
            templateText = Resources.Load<TextAsset>($"NGDT/Templates/{path}").text;
        }
        public PromptTemplate(TextAsset textAsset)
        {
            templateText = textAsset.text;
        }
        public string Get(Dictionary<string, object> inputs)
        {
            string output = templateText;
            foreach (var pair in inputs)
            {
                if (pair.Value is not string)
                {
                    output = output.Replace($"!<{pair.Key}>!", JsonConvert.SerializeObject(pair.Value));
                }
                else
                {
                    output = output.Replace($"!<{pair.Key}>!", (string)pair.Value);
                }
            }
            return output;
        }
        public string Get()
        {
            return templateText;
        }
    }
}
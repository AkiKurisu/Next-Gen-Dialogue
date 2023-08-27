using System.Collections.Generic;
using Newtonsoft.Json;
using System;
namespace Kurisu.NGDS.AI
{
    //Modify from https://github.com/THUDM/ChatGLM2-6B/blob/main/api.py
    public class GLMGenParams
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }
        [JsonProperty("history")]
        public List<string[]> History { get; set; }
    }
    [Serializable]
    public class GLMMessageBack
    {
        [JsonProperty("response")]
        public string Response { get; set; }
        [JsonProperty("history")]
        public List<string[]> History { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
    }
}
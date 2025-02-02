using System;
using Ceres.Annotations;
using Newtonsoft.Json;
namespace Kurisu.NGDT
{
    //Dummy editor module
    [Serializable]
    [CeresLabel("Load Oobabooga Session")]
    [ModuleOf(typeof(Dialogue))]
    [CeresGroup("Editor/AIGC")]
    [NodeInfo("Load Oobabooga session to graph")]
    public class OobaboogaSessionModule : EditorModule
    {

    }
    
    [Serializable]
    public class OobaboogaSession
    {
        public string name1;
        
        public string name2;
        
        public HistoryData history;
        
        public string context;
        
        public class HistoryData
        {
            [JsonProperty("internal")]
            public string[][] internalData;
        }
    }
}

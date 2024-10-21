using Newtonsoft.Json;
namespace Kurisu.NGDT
{
    //Dummy editor module
    [ModuleOf(typeof(Dialogue))]
    [NodeGroup("Editor/AIGC")]
    [NodeInfo("Load Oobabooga session to graph")]
    public class OobaboogaSessionModule : EditorModule
    {

    }
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

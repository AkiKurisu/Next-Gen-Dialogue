using System.Collections.Generic;
using System.Text;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Build value of string")]
    [AkiLabel("String: Build")]
    [AkiGroup("String")]
    public class BuildString : Action
    {
        public List<SharedString> values;
        [ForceShared]
        public SharedString storeResult;
        private readonly StringBuilder stringBuilder = new();
        public override void Awake()
        {
            foreach (var value in values) InitVariable(value);
        }
        protected override Status OnUpdate()
        {
            stringBuilder.Clear();
            for (int i = 0; i < values.Count; i++)
            {
                stringBuilder.Append(values[i].Value);
            }
            storeResult.Value = stringBuilder.ToString();
            return Status.Success;
        }
    }
}

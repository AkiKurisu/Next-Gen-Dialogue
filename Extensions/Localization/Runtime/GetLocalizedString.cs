using UnityEngine;
using UnityEngine.Localization;
namespace Kurisu.NGDT.Localization
{
    [NodeInfo("Action: Get LocalizedString")]
    [NodeLabel("String: GetLocalizedString")]
    [NodeGroup("String")]
    public class GetLocalizedString : Action
    {
        [WrapField]
        public LocalizedString localizedString;
        [ForceShared]
        public SharedString storeResult;
        [Setting, Tooltip("Toggle this to async get localizedString on start, so you can't changed result dynamically.")]
        public bool asyncGetOnStart;
        private string cache;
        public override void Awake()
        {
            if (asyncGetOnStart) LoadLocalizedStringAsync();
        }
        private async void LoadLocalizedStringAsync()
        {
            cache = await localizedString.GetLocalizedStringAsync().Task;
        }
        protected override Status OnUpdate()
        {
            storeResult.Value = asyncGetOnStart ? cache : localizedString.GetLocalizedString();
            return Status.Success;
        }
    }
}

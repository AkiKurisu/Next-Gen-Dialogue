using UnityEngine;
using UnityEngine.Localization;
namespace Kurisu.NGDT.Localization
{
    [AkiInfo("Action : Get LocalizedString")]
    [AkiLabel("String:GetLocalizedString")]
    [AkiGroup("String")]
    public class GetLocalizedString : Action
    {
        [SerializeField]
        private LocalizedString localizedString;
        [SerializeField, ForceShared]
        private SharedString storeResult;
        [SerializeField, Setting, Tooltip("Toggle this to async get localizedString on start, so you can't changed result dynamically.")]
        private bool asyncGetOnStart;
        private string cache;
        public override void Awake()
        {
            InitVariable(storeResult);
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

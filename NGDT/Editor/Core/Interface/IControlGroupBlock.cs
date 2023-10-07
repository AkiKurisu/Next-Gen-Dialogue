using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public interface IControlGroupBlock
    {
        void SelectGroup(IDialogueNode node);
        void UnSelectGroup();
        GroupBlock CreateBlock(Rect rect, GroupBlockData blockData = null);
    }
}

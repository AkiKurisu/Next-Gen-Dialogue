using System;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class NGDTDropdownMenuAction : DropdownMenuAction
    {
        public NGDTDropdownMenuAction(
            string actionName,
            Action<DropdownMenuAction> actionCallback,
            Func<DropdownMenuAction, Status> actionStatusCallback,
            object userData = null
        ) : base(actionName, actionCallback, actionStatusCallback, userData)
        {
        }

        public NGDTDropdownMenuAction(
            string actionName,
            Action<DropdownMenuAction> actionCallback
        ) : this(actionName, actionCallback, (e) => Status.Normal, null)
        {
        }
    }
}
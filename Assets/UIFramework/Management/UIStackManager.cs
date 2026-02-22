using System;
using System.Collections.Generic;

namespace Luzart.UIFramework
{
    public class UIStackManager
    {
        private readonly Stack<UIBase> screenStack = new Stack<UIBase>();
        private readonly Stack<UIBase> popupStack = new Stack<UIBase>();

        public int ScreenStackCount => screenStack.Count;
        public int PopupStackCount => popupStack.Count;

        public void PushScreen(UIBase screen)
        {
            if (screen == null)
                throw new ArgumentNullException(nameof(screen));

            screenStack.Push(screen);
        }

        public UIBase PopScreen()
        {
            return screenStack.Count > 0 ? screenStack.Pop() : null;
        }

        public UIBase PeekScreen()
        {
            return screenStack.Count > 0 ? screenStack.Peek() : null;
        }

        public void PushPopup(UIBase popup)
        {
            if (popup == null)
                throw new ArgumentNullException(nameof(popup));

            popupStack.Push(popup);
        }

        public UIBase PopPopup()
        {
            return popupStack.Count > 0 ? popupStack.Pop() : null;
        }

        public UIBase PeekPopup()
        {
            return popupStack.Count > 0 ? popupStack.Peek() : null;
        }

        public void ClearScreenStack()
        {
            screenStack.Clear();
        }

        public void ClearPopupStack()
        {
            popupStack.Clear();
        }

        public void ClearAll()
        {
            screenStack.Clear();
            popupStack.Clear();
        }

        public bool IsInScreenStack(UIBase screen)
        {
            return screenStack.Contains(screen);
        }

        public bool IsInPopupStack(UIBase popup)
        {
            return popupStack.Contains(popup);
        }
    }
}

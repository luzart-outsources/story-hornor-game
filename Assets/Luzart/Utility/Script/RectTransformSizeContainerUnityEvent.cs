using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Luzart
{
    public class RectTransformSizeContainerUnityEvent : MonoBehaviour
    {
        [SerializeField] private Mode mode;
        [SerializeField] private float time;
        [SerializeField] private int frame;
        [SerializeField] private UnityEvent unityEvent;
        void OnRectTransformDimensionsChange()
        {
            DoConstraint();
        }

        [ContextMenu("Constraint")]
        private void DoConstraint()
        {
            switch (mode)
            {
                case Mode.None:
                    {
                        CallUnityEvent();
                        break;
                    }
                case Mode.Frame:
                    {
                        GameUtil.Instance.WaitFrame(frame, CallUnityEvent);
                        break;
                    }
                case Mode.Second:
                    {
                        GameUtil.Instance.WaitAndDo(time, CallUnityEvent);
                        break;
                    }
            }
        }
        private void CallUnityEvent()
        {
            unityEvent?.Invoke();
        }

        public enum Mode
        {
            None = 0,
            Frame = 1,
            Second = 2,
        }
    }
}

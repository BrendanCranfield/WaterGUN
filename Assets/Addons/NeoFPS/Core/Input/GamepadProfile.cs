using NeoFPS.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [Serializable]
    public class GamepadProfile
    {
        [SerializeField, Tooltip("The name of the profile")]
        private string m_Name = string.Empty;
        [SerializeField, Tooltip("The analogue control order (look vs move)")]
        private AnalogueSetup m_AnalogueSetup = AnalogueSetup.LeftMoveRightLook;
        [SerializeField, Tooltip("The button mappings")]
        private FpsInputButton[] m_Buttons = { };

#if UNITY_EDITOR
        public bool expanded = true;
#endif

        public enum AnalogueSetup
        {
            LeftMoveRightLook,
            RightMoveLeftLook
        }

        public FpsInputButton[] buttons
        {
            get { return m_Buttons; }
        }

        public string name
        {
            get { return m_Name; }
        }

        public AnalogueSetup analogueSetup
        {
            get { return m_AnalogueSetup; }
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            int count = (int)GamepadButton.Count;
            if (m_Buttons.Length < count)
            {
                var swap = new FpsInputButton[count];

                int i = 0;
                for (; i < swap.Length && i < m_Buttons.Length; ++i)
                    swap[i] = m_Buttons[i];
                for (; i < swap.Length; ++i)
                    swap[i] = FpsInputButton.None;

                m_Buttons = swap;
            }
        }
#endif
    }
}
using System;
using UnityEngine;

namespace NeoFPS
{
    /// <summary>
    /// KeyBindingContext is used to track which FpsInputButton can clash and which must have unique key bindings.
    /// For example, right-click would be used for aiming with some weapons and secondary action with others.
    /// You can add to this if requried and then set the context in the m_KeyCodes entries.
    /// </summary>
    public enum KeyBindingContext
    {
        Default, // The key can be bound to this input and only this input
        Aimable, // The key cannot be used with other aimable or default inputs
        DualAction // The key cannot be used with other dualaction or default inputs
    }
}
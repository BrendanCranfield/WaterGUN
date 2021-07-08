using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
	public interface IThrownWeapon
	{
		void ThrowLight ();
		void ThrowHeavy ();

        float durationLight { get; }
        float durationHeavy { get; }

        event UnityAction onThrowLight;
        event UnityAction onThrowHeavy;
    }
}

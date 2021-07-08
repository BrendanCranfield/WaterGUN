using UnityEngine;
using System;
using UnityEngine.Events;


namespace NeoFPS
{
	public class ToggleOrHoldEvent : ToggleOrHold
	{
		public ToggleOrHoldEvent()
		{}

		public ToggleOrHoldEvent(UnityAction onAct, UnityAction onDeact)
		{
			onActivate += onAct;
			onDeactivate += onDeact;
		}

		public event UnityAction onActivate;
		public event UnityAction onDeactivate;

		protected override void OnActivate ()
		{
			if (onActivate != null)
				onActivate ();
		}

		protected override void OnDeactivate ()
		{
			if (onDeactivate != null)
				onDeactivate ();
		}
	}
}
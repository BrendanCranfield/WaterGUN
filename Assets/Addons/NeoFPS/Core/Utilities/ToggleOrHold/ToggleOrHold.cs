using System;
using UnityEngine;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
	/// <summary>
	/// A simple class for tracking on/off inputs in either toggle mode or hold down.
	/// Also allows for simple reference counted blockers that can interrupt and toggle the output, though care must be taken to make sure each blocker is removed when appropriate.
	/// For a safer but less optimal implementation that allows tracking of specific blockers, use the SafeToggleOrHold base class.
	/// </summary>
	public class ToggleOrHold
	{
        private int m_Blockers = 0;
		public int blockers
		{
			get { return m_Blockers; }
			private set
			{
				// Check for difference
				if (m_Blockers != value)
                {
                    bool previous = on;

					// Set the new value (and check for negative)
					m_Blockers = value;
					if (m_Blockers < 0)
					{
						Debug.LogError ("Negative blocker count for toggle/hold. Check for inconsistent Add/Remove/Reset calls.");
						m_Blockers = 0;
					}

                    // Handle blocking when input is on
                    bool result = on;
                    if (result != previous)
					{
                        if (result)
                            OnActivate();
                        else
                            OnDeactivate();
					}
				}
			}
		}

        private bool m_Hold = false;
        private bool m_On = false;
		public bool on
		{
			get { return m_Blockers == 0 && (m_On || m_Hold); }
			set
			{
				// Check for difference
				if (m_On != value)
                {
                    bool previous = on;

                    // Set value
                    m_On = value;

                    // Handle change
                    bool result = on;
                    if (m_Blockers == 0 && result != previous)
                    {
                        if (result)
                            OnActivate();
                        else
                            OnDeactivate();
                    }
                }
			}
		}

        /// <summary>
        /// Add a specific blocker. Each call should have an associated RemoveBlocker call as well to maintain balance.
        /// </summary>
        public void AddBlocker ()
		{
			++blockers;
		}

		/// <summary>
		/// Removes the blocker. This should only be called if a blocker has been added and is no longer needed.
		/// </summary>
		public void RemoveBlocker ()
		{
			--blockers;
            if (blockers < 0)
                blockers = 0;
		}

        public void Toggle()
        {
            bool previous = on;

            m_On = !m_On;

            bool result = on;
            if (m_Blockers == 0 && result != previous)
            {
                if (result)
                    OnActivate();
                else
                    OnDeactivate();
            }
        }

        public void Hold(bool hold = true)
        {
            bool previous = on;

            m_Hold = hold;

            bool result = on;
            if (m_Blockers == 0 && result != previous)
            {
                if (result)
                    OnActivate();
                else
                    OnDeactivate();
            }
        }

        public void SetInput(bool toggle, bool hold)
        {
            bool previous = on;

			if (toggle)
                m_On = !m_On;
            m_Hold = hold;

            bool result = on;
            if (m_Blockers == 0 && result != previous)
            {
                if (result)
                    OnActivate();
                else
                    OnDeactivate();
            }
        }

        protected virtual void OnActivate () {}
		protected virtual void OnDeactivate () {}

        #region INeoSerializableObject IMPLEMENTATION

        private static readonly NeoSerializationKey k_OnKey = new NeoSerializationKey("on");
        private static readonly NeoSerializationKey k_HoldKey = new NeoSerializationKey("hold");
        private static readonly NeoSerializationKey k_BlockersKey = new NeoSerializationKey("blockers");

        public void WriteProperties(INeoSerializer writer, string key, bool writeBlockers)
        {
            WriteProperties(writer, Animator.StringToHash(key), writeBlockers);
        }

        public void ReadProperties(INeoDeserializer reader, string key)
        {
            ReadProperties(reader, Animator.StringToHash(key));
        }

        public void WriteProperties(INeoSerializer writer, int hash, bool writeBlockers)
        {
            writer.PushContext(SerializationContext.ObjectNeoSerialized, hash);

            writer.WriteValue(k_OnKey, m_On);
            writer.WriteValue(k_HoldKey, m_Hold);

            if (writeBlockers)
                writer.WriteValue(k_BlockersKey, m_Blockers);

            writer.PopContext(SerializationContext.ObjectNeoSerialized);
        }

        public void ReadProperties(INeoDeserializer reader, int hash)
        {
            if (reader.PushContext(SerializationContext.ObjectNeoSerialized, hash))
            {
                reader.TryReadValue(k_OnKey, out m_On, m_On);
                reader.TryReadValue(k_HoldKey, out m_Hold, m_Hold);

                // Read blockers
                int blockerCount;
                if (reader.TryReadValue(k_BlockersKey, out blockerCount, 0))
                    blockers = blockerCount;

                reader.PopContext(SerializationContext.ObjectNeoSerialized, hash);
            }
        }

        #endregion
    }
}
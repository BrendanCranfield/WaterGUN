using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-targettrackingammoeffect.html")]
    public class TargetTrackingAmmoEffect : BaseAmmoEffect, ITargetingSystem, INeoSerializableComponent
    {
        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false, false), Tooltip("An optional secondary ammo effect to allow the tracking bullet to deal damage, etc.")]
        private BaseAmmoEffect m_SecondaryEffect = null;

        [SerializeField, Tooltip("The amount of time (seconds) the tracking effect will last for.")]
        private float m_TrackerLifetime = 100f;

        private List<ITargetTracker> m_ActiveTrackers = new List<ITargetTracker>();
        private WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();
        private Transform m_TrackedTransform = null;
        private Vector3 m_RelativePosition = Vector3.zero;
        private float m_LifetimeRemaining = 0f;
        private Coroutine m_TimeoutCoroutine = null;

        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Apply the hit effect
            if (m_SecondaryEffect != null)
                m_SecondaryEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);

            // Get the hit collider's transform
            m_TrackedTransform = hit.collider.transform;

            // Get the relative position
            m_RelativePosition = hit.point - m_TrackedTransform.position;
            m_RelativePosition = Quaternion.Inverse(m_TrackedTransform.rotation) * m_RelativePosition;

            // Apply to active trackers
            for (int i = 0; i < m_ActiveTrackers.Count; ++i)
                m_ActiveTrackers[i].SetTargetTransform(m_TrackedTransform, m_RelativePosition, false);
        }

        public void RegisterTracker(ITargetTracker tracker)
        {
            // Record tracker
            m_ActiveTrackers.Add(tracker);
            tracker.onDestroyed += OnTrackerDestroyed;

            // Set target if one exists
            if (m_TrackedTransform != null)
                tracker.SetTargetTransform(m_TrackedTransform, m_RelativePosition, false);
            else
                tracker.ClearTarget();

            // Reset timeout
            m_LifetimeRemaining = m_TrackerLifetime;
            if (m_TimeoutCoroutine == null)
                m_TimeoutCoroutine = StartCoroutine(TimeoutCoroutine());
        }

        void OnTrackerDestroyed(ITargetTracker tracker)
        {
            tracker.onDestroyed -= OnTrackerDestroyed;
            m_ActiveTrackers.Remove(tracker);
        }

        void OnDisable()
        {
            for (int i = 0; i < m_ActiveTrackers.Count; ++i)
                m_ActiveTrackers[i].onDestroyed -= OnTrackerDestroyed;
            m_ActiveTrackers.Clear();
            m_TrackedTransform = null;
        }

        IEnumerator TimeoutCoroutine()
        {
            // Countdown lifetime
            while (m_LifetimeRemaining > 0f)
            {
                yield return m_WaitForFixedUpdate;
                m_LifetimeRemaining -= Time.deltaTime;
            }

            // reset to zero
            m_LifetimeRemaining = 0f;

            // Clear targets
            for (int i = 0; i < m_ActiveTrackers.Count; ++i)
                m_ActiveTrackers[i].ClearTarget();
            m_TrackedTransform = null;

            // Release coroutine
            m_TimeoutCoroutine = null;
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_ActiveTrackersKey = new NeoSerializationKey("activeTrackers");
        private static readonly NeoSerializationKey k_LifetimeKey = new NeoSerializationKey("lifetime");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_ActiveTrackers.Count > 0)
            {
                // Push a context (workaround for no read/write obj reference arrays)
                writer.PushContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey);

                // Write object references
                for (int i = 0; i < m_ActiveTrackers.Count; ++i)
                    writer.WriteComponentReference(i, m_ActiveTrackers[i], nsgo);

                // Pop context
                writer.PopContext(SerializationContext.ObjectNeoSerialized);
            }

            writer.WriteValue(k_LifetimeKey, m_LifetimeRemaining);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Push a context (workaround for no read/write obj reference arrays)
            // If context isn't found, the array was empty
            if (reader.PushContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey))
            {
                // Read object references
                for (int i = 0; true; ++i)
                {
                    ITargetTracker tracker;
                    if (reader.TryReadComponentReference(i, out tracker, null))
                        m_ActiveTrackers.Add(tracker);
                    else
                        break;
                }

                reader.PopContext(SerializationContext.ObjectNeoSerialized, k_ActiveTrackersKey);
            }

            if (reader.TryReadValue(k_LifetimeKey, out m_LifetimeRemaining, m_LifetimeRemaining))
            {
                if (m_LifetimeRemaining > 0f && m_TimeoutCoroutine == null)
                    m_TimeoutCoroutine = StartCoroutine(TimeoutCoroutine());
            }
        }

        #endregion
    }
}
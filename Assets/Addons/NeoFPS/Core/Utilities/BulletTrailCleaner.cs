using NeoFPS.ModularFirearms;
using System.Collections;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(TrailRenderer))]
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-bullettrailcleaner.html")]
    public class BulletTrailCleaner : MonoBehaviour
    {
        private TrailRenderer m_TrailRenderer = null;

        private void Awake()
        {
            m_TrailRenderer = GetComponent<TrailRenderer>();
            var projectile = GetComponent<IProjectile>();
            if (projectile != null)
                projectile.onTeleported += OnTeleported;
        }

        [SerializeField, Tooltip("The delay before enabling the trail render")]
        private float m_EmitDelay = 0f;

        void OnEnable()
        {
            m_TrailRenderer.emitting = false;
            m_TrailRenderer.Clear();
            StartCoroutine(DelayedEnableTrail());
        }

        void OnTeleported()
        {
            m_TrailRenderer.Clear();
        }

        IEnumerator DelayedEnableTrail()
        {
            float timer = 0f;
            do
            {
                yield return null;
                timer += Time.deltaTime;
            }
            while (timer < m_EmitDelay);

            m_TrailRenderer.emitting = true;
        }
    }
}

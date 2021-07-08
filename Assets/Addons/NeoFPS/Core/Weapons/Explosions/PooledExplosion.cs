using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections.Generic;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-explosion.html")]
    [RequireComponent(typeof(PooledObject))]
    public class PooledExplosion : MonoBehaviour, IDamageSource, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The valid collision layers the explosion will affect")]
        private LayerMask m_CollisionLayers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("Duration the object should remain active before being returned to the pool.")]
        private float m_Lifetime = 2f;

        [Header("Damage")]

        [SerializeField, Tooltip("A description of the damage, used when logging and displaying game events.")]
        private string m_PrintableName = "Explosion";

        [SerializeField, Tooltip("The damage type the explosion applies (enables filtering damage types).")]
        private DamageType m_DamageType = DamageType.Explosion;

        [SerializeField, Tooltip("The radius of the explosion")]
        private float m_Radius = 1f;

        [Header("Shake")]

        [SerializeField, Tooltip("The strength of the camera (and other) shake due to the explosion.")]
        private float m_ShakeStrength = 0.5f;

        [SerializeField, Tooltip("The inner shake radius of the explosion. Any shake handlers within this radius will be affected at full strength, falling off to 0 outside this based on the falloff distance.")]
        private float m_ShakeInnerRadius = 10f;

        [SerializeField, Tooltip("The distance beyond the inner radius where the shake effect drops off to 0.")]
        private float m_ShakeFalloffDistance = 10f;

        [SerializeField, Tooltip("The distance beyond the inner radius where the shake effect drops off to 0.")]
        private float m_ShakeDuration = 0.75f;

        const int k_MaxHits = 128;

        private static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(8);
        private static Collider[] s_HitColliders = new Collider[k_MaxHits];

        private PooledObject m_PooledObject = null;
        private float m_Timer = 0f;

        public DamageType damageType
        {
            get { return m_DamageType; }
            set { m_DamageType = value; }
        }

        public float radius
        {
            get { return m_Radius; }
            set { m_Radius = value; }
        }
        
        private void OnValidate()
        {
            if (m_Radius < 0.1f)
                m_Radius = 0.1f;
            if (m_ShakeInnerRadius < 0.1f)
                m_ShakeInnerRadius = 0.1f;
            if (m_ShakeFalloffDistance < 0f)
                m_ShakeFalloffDistance = 0f;
            m_ShakeStrength = Mathf.Clamp(m_ShakeStrength, 0f, 10f);
            m_Lifetime = Mathf.Clamp(m_Lifetime, 0.25f, 100f);
        }

        private void Awake()
        {
            m_PooledObject = GetComponent<PooledObject>();
        }

        private void OnEnable()
        {
            m_Timer = 0f;
        }

        private void Update()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer > m_Lifetime)
                m_PooledObject.ReturnToPool();
        }

        public virtual void Explode(float maxDamage, float maxForce, IDamageSource source = null, Transform ignoreRoot = null)
        {
            if (source != null)
            {
                controller = source.controller;
                m_OutDamageFilter = new DamageFilter(m_DamageType, source.outDamageFilter.GetTeamFilter());
            }
            else
            {
                controller = null;
                m_OutDamageFilter = new DamageFilter(m_DamageType, DamageTeamFilter.All);
            }

            var t = transform;
            Vector3 position = t.position;

            RaycastHit coverCheck = new RaycastHit();

            int hitCount = Physics.OverlapSphereNonAlloc(position, m_Radius, s_HitColliders);
            for (int i = 0; i < hitCount; ++i)
            {
                // Check if hit should be ignored
                if (ignoreRoot != null)
                {
                    bool ignore = false;

                    Transform itr = s_HitColliders[i].transform;
                    while (itr != null)
                    {
                        if (itr == ignoreRoot)
                        {
                            ignore = true;
                            break;
                        }
                        itr = itr.parent;
                    }

                    if (ignore)
                        continue;
                }

                Vector3 targetPosition = s_HitColliders[i].transform.position;
                Vector3 direction = targetPosition - position;
                float distance = direction.magnitude;

                if (PhysicsExtensions.RaycastNonAllocSingle(new Ray(position, direction / distance), out coverCheck, m_Radius, m_CollisionLayers, null, QueryTriggerInteraction.Ignore))
                {
                    ApplyExplosionEffect(coverCheck, position, maxDamage, maxForce);
                }
            }

            // Apply shake
            if (m_ShakeStrength > 0f)
                ShakeHandler.Shake(position, m_ShakeInnerRadius, m_ShakeFalloffDistance, m_ShakeStrength, m_ShakeDuration);
        }

        protected virtual void ApplyExplosionEffect(RaycastHit hit, Vector3 center, float maxDamage, float maxForce)
        {
            float falloff = 1f - Mathf.Clamp01(hit.distance / m_Radius);
            Collider c = hit.collider;
            			
            // Apply damage
            if (maxDamage > 0f)
            {
                // Apply damage
                float damage = falloff * maxDamage;
                c.GetComponents(s_DamageHandlers);
                for (int i = 0; i < s_DamageHandlers.Count; ++i)
                    s_DamageHandlers[i].AddDamage(damage, this);
                s_DamageHandlers.Clear();
            }

            // Apply force
            if (c != null && maxForce > 0f)
            {
                bool hasRigidBody = hit.rigidbody != null && !hit.rigidbody.isKinematic;
                if (hasRigidBody)
                {
                    hit.rigidbody.AddExplosionForce(maxForce, hit.point, m_Radius, 0.25f, ForceMode.Impulse);
                }
                else
                {
                    IImpactHandler impactHandler = c.GetComponent<IImpactHandler>();
                    if (impactHandler != null)
                    {
                        var direction = (hit.point - center).normalized;
                        impactHandler.HandlePointImpact(hit.point, direction * maxForce * falloff);
                    }
                }
            }
        }

        #region IDamageSource IMPLEMENTATION

        private DamageFilter m_OutDamageFilter = DamageFilter.DefaultAllTeams;
        public DamageFilter outDamageFilter
        { 
            get { return m_OutDamageFilter; }
            set { m_OutDamageFilter = value; }
        }

        public IController controller
        {
            get;
            private set;
        }

        public Transform damageSourceTransform
        {
            get { return transform; }
        }

        public string description
        {
            get { return m_PrintableName; }
        }

        #endregion

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_FilterKey = new NeoSerializationKey("outFilter");
        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_TimerKey, m_Timer);
            writer.WriteValue(k_FilterKey, outDamageFilter);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
            ushort filter;
            if (reader.TryReadValue(k_TimerKey, out filter, 0))
                outDamageFilter = filter;
        }

        #endregion
    }
}

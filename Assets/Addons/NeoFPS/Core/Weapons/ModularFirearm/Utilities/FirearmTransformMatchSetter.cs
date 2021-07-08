using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(ModularFirearm))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-firearmtransformmatchsetter.html")]
    public class FirearmTransformMatchSetter : MonoBehaviour
    {
        [SerializeField, Tooltip("The target transform to match.")]
        private Transform m_Target = null;

        [SerializeField, Tooltip("The transform to use when calculating the offset of the target transform.")]
        private Transform m_RelativeTo = null;

        [SerializeField, Range(0f, 1f), Tooltip("The strength of the effect. 1 matches the movement absolutely, while 0 is no movement.")]
        private float m_Weight = 1f;

        private ModularFirearm m_Firearm;
        private TransformMatcher m_TransformMatcher;

        private void Awake()
        {
            m_Firearm = GetComponent<ModularFirearm>();
        }

        private void OnEnable()
        {
            if (m_Firearm.wielder == null)
                return;
            if (m_Firearm.wielder.headTransformHandler == null)
                return;

            m_TransformMatcher = m_Firearm.wielder.headTransformHandler.GetAdditiveTransform<TransformMatcher>();
            if (m_TransformMatcher != null)
                m_TransformMatcher.SetTargetTransforms(m_Target, m_RelativeTo, m_Weight);
            else
                Debug.Log("Couldn't find transform matcher");
        }

        private void OnDisable()
        {
            if (m_TransformMatcher != null)
                m_TransformMatcher.ClearTargetTransforms();
        }
    }
}

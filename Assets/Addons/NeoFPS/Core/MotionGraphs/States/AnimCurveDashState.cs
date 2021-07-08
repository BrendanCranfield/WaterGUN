#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Dashes/Anim-Curve Dash", "Dash (Anim Curve)")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-animcurvedashstate.html")]
    public class AnimCurveDashState : MotionGraphState
    {
        [SerializeField, Tooltip("The target speed for the dash to reach. This will be layered on top of the control speed.")]
        private FloatDataReference m_DashSpeed = new FloatDataReference(50f);
        [SerializeField, Tooltip("The maximum speed the character can reach under motor control (driven by input). The dash velocity will be layered on top of this.")]
        private FloatDataReference m_MaxControlSpeed = new FloatDataReference(5f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed when strafing")]
        private FloatDataReference m_StrafeMultiplier = new FloatDataReference(0.75f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed when moving in reverse")]
        private FloatDataReference m_ReverseMultiplier = new FloatDataReference(0.5f);
        [SerializeField, Tooltip("The maximum acceleration")]
        private FloatDataReference m_Acceleration = new FloatDataReference(50f);
        [SerializeField, Tooltip("The direction to base the dash off. This can be **Yaw Relative** or **Move Relative**.")]
        private DashDirection m_FrameOfReference = DashDirection.YawRelative;
        [SerializeField, Tooltip("The angle offset for the dash direction. For example, yaw relative and an angle of 90 will dash to the right. -90 will dash to the left.")]
        private float m_DashAngle = 0f;
        [SerializeField, Tooltip("The amount of time it takes to reach the dash speed. At this point, the animation curve kicks in to ease out of the dash. A Dash In Time of 0 is instant.")]
        private float m_DashInTime = 0.05f;
        [SerializeField, Tooltip("The amount of time it takes for the animation curve kicks to ease out of the dash.")]
        private float m_DashOutTime = 0.5f;
        [SerializeField, Tooltip("The ease out curve for the dash velocity. This should start at 1. Dipping below zero will mean the dash is moving backwards.")]
        private AnimationCurve m_DashOutCurve = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(0f, 1f), new Keyframe(0.1f, 1f), new Keyframe(0.7f, -0.05f), new Keyframe(0.9f, 0.02f), new Keyframe(1f, 0f)
        });
        [SerializeField, Tooltip("Should the character fall with gravity during the dash")]
        private bool m_ApplyGravity = true;
        [SerializeField, Range(0f, 1f), Tooltip("The amount of damping to apply to the controlled velocity when changing direction")]
        private float m_ControlDamping = 0.25f;

        private const float k_TinyValue = 0.001f;

        public enum DashDirection
        {
            YawRelative,
            MoveRelative
        }

        private Vector3 m_DashHeading = Vector3.zero;
        private Vector3 m_MoveAcceleration = Vector3.zero;
        private Vector3 m_MoveVelocity = Vector3.zero;
        private Vector3 m_DashVelocity = Vector3.zero;
        private Vector3 m_CombinedVelocity = Vector3.zero;
        private float m_LerpIn = 0f;
        private float m_LerpOut = 0f;
        private float m_PreviousMoveSpeed = 0f;
        private bool m_Completed = false;

        public override bool completed
        {
            get { return m_Completed; }
        }

        public override Vector3 moveVector
        {
            get { return m_CombinedVelocity * Time.deltaTime; }
        }

        public override bool applyGravity
        {
            get { return m_ApplyGravity; }
        }

        public override bool applyGroundingForce
        {
            get { return characterController.isGrounded; }
        }

        public override bool ignorePlatformMove
        {
            get { return false; }
        }

        public override void OnValidate()
        {
            base.OnValidate();

            var keys = m_DashOutCurve.keys;
            if (keys.Length < 2)
            {
                var newKeys = new Keyframe[2];
                if (keys.Length == 1)
                    newKeys[0] = keys[0];
                else
                    newKeys[0] = new Keyframe(0f, 1f);
                newKeys[1] = new Keyframe(1f, 0f);
                m_DashOutCurve.keys = newKeys;
            }
            else
            {
                keys[0].time = 0f;
                keys[keys.Length - 1].time = 1f;
            }

            m_DashInTime = Mathf.Clamp(m_DashInTime, 0f, 10f);
            m_DashOutTime = Mathf.Clamp(m_DashOutTime, 0.05f, 10f);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_LerpOut = 0f;
            m_Completed = false;
            m_DashVelocity = Vector3.zero;
            m_MoveVelocity = characterController.velocity;
            m_PreviousMoveSpeed = m_MoveVelocity.magnitude;
            m_MoveAcceleration = Vector3.zero;

            // Reset lerp in (skip if lerp takes less than 1 frame)
            m_LerpIn = 0f;

            // Get heading
            m_DashHeading = Vector3.zero;
            if (m_FrameOfReference == DashDirection.YawRelative)
                m_DashHeading = characterController.forward;
            else
                m_DashHeading = Vector3.ProjectOnPlane(characterController.velocity, characterController.up).normalized;

            if (Mathf.Abs(m_DashAngle) > 0.1f)
                m_DashHeading = Quaternion.AngleAxis(m_DashAngle, characterController.up) * m_DashHeading;
        }

        public override void OnExit()
        {
            base.OnExit();

            m_Completed = false;
            m_DashHeading = Vector3.zero;
            m_DashVelocity = Vector3.zero;
            m_MoveVelocity = Vector3.zero;
            m_MoveAcceleration = Vector3.zero;
            m_LerpOut = 0f;
        }

        public override void Initialise(IMotionController c)
        {
            base.Initialise(c);
            m_DashOutCurve.preWrapMode = WrapMode.ClampForever;
            m_DashOutCurve.postWrapMode = WrapMode.ClampForever;
        }

        public override void Update()
        {
            base.Update();

            float dashSpeed = 0f;

            //Sort dash velocity
            if (m_LerpIn < 1f)
            {
                if (m_DashInTime < Time.fixedDeltaTime)
                    m_LerpIn = 1f;
                else
                {
                    m_LerpIn += Time.deltaTime / m_DashInTime;
                    if (m_LerpIn > 1f)
                        m_LerpIn = 1f;
                }

                dashSpeed = EasingFunctions.EaseInQuadratic(m_LerpIn) * m_DashSpeed.value;
                m_DashVelocity = m_DashHeading * dashSpeed;
            }
            else
            {
                m_LerpOut += Time.deltaTime / m_DashOutTime;
                if (m_LerpOut > 1f)
                {
                    m_LerpOut = 1f;
                    m_Completed = true;
                }

                m_DashHeading = characterController.velocity.normalized;

                dashSpeed = m_DashOutCurve.Evaluate(m_LerpOut) * m_DashSpeed.value;
                m_DashVelocity = m_DashHeading * dashSpeed;
            }

            // Get character velocity
            Vector3 up = characterController.up;
            Vector3 hVelocity = Vector3.ProjectOnPlane(m_MoveVelocity, up);

            float topSpeed = m_MaxControlSpeed.value;
            float directionMultiplier = 1f;
            Vector3 targetVelocity = m_MoveVelocity;// Vector3.zero;

            if (controller.inputMoveScale > k_TinyValue)
            {
                // Calculate speed based on move direction
                if (controller.inputMoveDirection.y < 0f)
                    directionMultiplier *= Mathf.Lerp(1f, m_ReverseMultiplier.value, -controller.inputMoveDirection.y);
                directionMultiplier *= Mathf.Lerp(1f, m_StrafeMultiplier.value, Mathf.Abs(controller.inputMoveDirection.x));

                // Get the input based move direction
                Vector3 direction = characterController.forward * controller.inputMoveDirection.y;
                direction += characterController.right * controller.inputMoveDirection.x;

                // Get the target speed based on the input & current speed in the desired direction
                float inputSpeed = topSpeed * controller.inputMoveScale * directionMultiplier;
                float alignedSpeed = Vector3.Dot(hVelocity, direction);

                // Clamp top speed
                //if (alignedSpeed > topSpeed)
                //    alignedSpeed = topSpeed;

                // Get the target vector
                targetVelocity = direction * Mathf.Max(inputSpeed, alignedSpeed);
            }

            // Change velocity
            float hAcceleration = m_Acceleration.value;
            if (hAcceleration < k_TinyValue)
            {
                // Don't use acceleration (instant)
                m_MoveVelocity = targetVelocity;
            }
            else
            {
                // Get maximum acceleration
                float maxAccel = hAcceleration * directionMultiplier;
                // Accelerate the velocity
                m_MoveVelocity = Vector3.SmoothDamp(hVelocity, targetVelocity, ref m_MoveAcceleration, Mathf.Lerp(0.05f, 0.25f, m_ControlDamping), maxAccel);
            }

            m_CombinedVelocity = m_MoveVelocity + m_DashVelocity;
            m_CombinedVelocity = Vector3.ClampMagnitude(m_CombinedVelocity, m_PreviousMoveSpeed + dashSpeed);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_DashSpeed.CheckReference(map);
            m_Acceleration.CheckReference(map);
            m_MaxControlSpeed.CheckReference(map);
            m_StrafeMultiplier.CheckReference(map);
            m_ReverseMultiplier.CheckReference(map);

            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_HeadingKey = new NeoSerializationKey("heading");
        private static readonly NeoSerializationKey k_MoveAccelKey = new NeoSerializationKey("moveAccel");
        private static readonly NeoSerializationKey k_MoveVelKey = new NeoSerializationKey("moveVel");
        private static readonly NeoSerializationKey k_DashVelKey = new NeoSerializationKey("dashVel");
        private static readonly NeoSerializationKey k_LerpInKey = new NeoSerializationKey("lerpIn");
        private static readonly NeoSerializationKey k_LerpOutKey = new NeoSerializationKey("lerpOut");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_HeadingKey, m_DashHeading);
            writer.WriteValue(k_MoveAccelKey, m_MoveAcceleration);
            writer.WriteValue(k_MoveVelKey, m_MoveVelocity);
            writer.WriteValue(k_DashVelKey, m_DashVelocity);
            writer.WriteValue(k_LerpInKey, m_LerpIn);
            writer.WriteValue(k_LerpOutKey, m_LerpOut);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_HeadingKey, out m_DashHeading, m_DashHeading);
            reader.TryReadValue(k_MoveAccelKey, out m_MoveAcceleration, m_MoveAcceleration);
            reader.TryReadValue(k_MoveVelKey, out m_MoveVelocity, m_MoveVelocity);
            reader.TryReadValue(k_DashVelKey, out m_DashVelocity, m_DashVelocity);
            reader.TryReadValue(k_LerpInKey, out m_LerpIn, m_LerpIn);
            reader.TryReadValue(k_LerpOutKey, out m_LerpOut, m_LerpOut);
        }

        #endregion
    }
}
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_moveInputRef;

    [SerializeField, Min(0.01f)]
    private float m_speed = 4f;

    [SerializeField]
    private NavMeshAgent m_agent;

    [SerializeField]
    private Animator m_animator;

    [SerializeField, Min(0f)]
    private float m_turnSpeed = 720f;

    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    [SerializeField]
    private Transform m_camera;

    private void Awake()
    {
        m_agent.speed = m_speed;
        m_agent.updateRotation = false;
        m_animator.applyRootMotion = false;
        m_moveInputRef.action.Enable();
    }

    private void Update()
    {
        if (!m_agent.isActiveAndEnabled || !m_agent.isOnNavMesh)
        {
            m_animator.SetFloat(SpeedParameter, 0f);
            return;
        }

        Vector2 input = Vector2.ClampMagnitude(m_moveInputRef.action.ReadValue<Vector2>(), 1f);

        Vector3 cameraForwardFlat = m_camera != null ? m_camera.forward : Vector3.forward;
        cameraForwardFlat.y = 0f;
        cameraForwardFlat = cameraForwardFlat.sqrMagnitude > 0.001f ? cameraForwardFlat.normalized : Vector3.forward;
        Vector3 cameraRightFlat = Vector3.Cross(Vector3.up, cameraForwardFlat);
        Vector3 direction = cameraRightFlat * input.x + cameraForwardFlat * input.y;

        m_agent.velocity = direction * m_speed;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            m_agent.transform.rotation = Quaternion.RotateTowards(m_agent.transform.rotation, targetRotation, m_turnSpeed * Time.deltaTime);
        }

        Vector3 velocity = m_agent.velocity;
        velocity.y = 0f;
        float animationSpeed = Mathf.Clamp01(velocity.magnitude / Mathf.Max(m_speed, 0.01f));
        m_animator.SetFloat(SpeedParameter, animationSpeed, 0.1f, Time.deltaTime);
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StepSound : MonoBehaviour
{
    [SerializeField]
    private AudioSource m_audioSource;

    [SerializeField]
    private List<AudioClip> m_stepSounds;

    private const float MinimumMovementSpeed = 0.05f;

    private NavMeshAgent m_agent;

    private void Awake()
    {
        m_agent = GetComponentInParent<NavMeshAgent>();

        if (m_audioSource == null)
            m_audioSource = GetComponent<AudioSource>();
    }

    private bool IsMoving()
    {
        if (m_agent == null || !m_agent.isActiveAndEnabled || !m_agent.isOnNavMesh)
            return false;

        Vector3 velocity = m_agent.velocity;
        velocity.y = 0f;

        return velocity.sqrMagnitude > MinimumMovementSpeed * MinimumMovementSpeed;
    }

    public void Step()
    {
        if (!isActiveAndEnabled || !IsMoving() || m_audioSource == null ||
            m_stepSounds == null || m_stepSounds.Count == 0)
            return;

        AudioClip clip = m_stepSounds[Random.Range(0, m_stepSounds.Count)];
        if (clip != null)
            m_audioSource.PlayOneShot(clip);
    }

    private void LateUpdate()
    {
        if (m_audioSource != null && m_audioSource.isPlaying && !IsMoving())
            m_audioSource.Stop();
    }

    private void OnDisable()
    {
        if (m_audioSource != null)
            m_audioSource.Stop();
    }
}
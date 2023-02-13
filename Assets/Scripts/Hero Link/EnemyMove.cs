using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
    // Variables de vitesse
    [SerializeField, Tooltip("Multipicateur de la velocity, utilisé dans l'animator"), Range(0.1f, 1f)] private float _speedAnimMultiplier;
    [SerializeField] private AudioSource _footstepsAudioSource;

    // Références de composants
    private Transform _transform;
    private NavMeshAgent _agent;
    private Animator _animator;
    private AudioSource _audioSource;

    // Propriétés publiques
    public NavMeshAgent Agent { get => _agent; set => _agent = value; }

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // On envoie la vitesse de déplacement à l'Animator
        _animator.SetFloat("Speed", _agent.velocity.magnitude * _speedAnimMultiplier);
        _footstepsAudioSource.pitch = _agent.velocity.magnitude * 2f / _agent.speed ;

    }

    public void ChangeTarget(Transform target)
    {
        _agent.isStopped = true;
        _agent.destination = target.position;
    }

    public void StartAgent()
    {
        _agent.isStopped = false;
    }

    public void StopAgent()
    {
        _agent.isStopped = true;
    }
}

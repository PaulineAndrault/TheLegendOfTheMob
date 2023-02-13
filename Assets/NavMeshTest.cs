using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    public Transform Target;

    // Références de composants
    private Transform _transform;
    private NavMeshAgent _agent;
    private Animator _animator;

    // Propriétés publiques
    public NavMeshAgent Agent { get => _agent; set => _agent = value; }

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _agent.isStopped = true;
        _agent.destination = Target.position;
        _agent.isStopped = false;
    }
}

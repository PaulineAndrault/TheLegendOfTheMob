using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementState
{
    IDLE,
    WALK,
}

public class PlayerMoveStateMachine : MonoBehaviour
{
    [SerializeField] private MovementState _currentState;
    private Animator _animator;
    private PlayerInput _playerInput;
    private PlayerController _playerController;
    private PlayerAirStateMachine _playerAirStateMachine;


    public MovementState CurrentState { get => _currentState; }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerController = GetComponent<PlayerController>();
        _playerAirStateMachine = GetComponent<PlayerAirStateMachine>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        OnStateEnter(MovementState.IDLE);
    }
    private void Update()
    {
        OnStateUpdate(_currentState);
    }

    #region STATE MACHINE PATTERN
    private void OnStateEnter(MovementState state)
    {
        switch (state)
        {
            case MovementState.IDLE:
                OnEnterIdle();
                break;
            case MovementState.WALK:
                OnEnterWalk();
                break;
            default:
                Debug.LogError("OnStateEnter: Invalid state " + state.ToString());
                break;
        }
    }
    private void OnStateUpdate(MovementState state)
    {
        switch (state)
        {
            case MovementState.IDLE:
                OnUpdateIdle();
                break;
            case MovementState.WALK:
                OnUpdateWalk();
                break;
            default:
                Debug.LogError("OnStateUpdate: Invalid state " + state.ToString());
                break;
        }
    }
    private void OnStateExit(MovementState state)
    {
        switch (state)
        {
            case MovementState.IDLE:
                OnExitIdle();
                break;
            case MovementState.WALK:
                OnExitWalk();
                break;
            default:
                Debug.LogError("OnStateExit: Invalid state " + state.ToString());
                break;
        }
    }
    public void TransitionToState(MovementState toState)
    {
        OnStateExit(_currentState);
        _currentState = toState;
        OnStateEnter(toState);
    }

    #endregion

    #region IDLE
    private void OnEnterIdle()
    {
        _animator.SetBool("Idle", true);
    }
    private void OnUpdateIdle()
    {
        if (_playerInput.Movement.HasMovement)
        {
            TransitionToState(MovementState.WALK);
        }
    }
    private void OnExitIdle()
    {
        _animator.SetBool("Idle", false);
    }
    #endregion

    #region WALK
    private void OnEnterWalk()
    {
        _animator.SetBool("Walk", true);
    }
    private void OnUpdateWalk()
    {
        if (!_playerInput.Movement.HasMovement)
        {
            TransitionToState(MovementState.IDLE);
        }
    }
    private void OnExitWalk()
    {
        _animator.SetBool("Walk", false);
    }
    #endregion
}

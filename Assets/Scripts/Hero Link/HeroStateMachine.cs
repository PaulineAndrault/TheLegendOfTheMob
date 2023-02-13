using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroState
{
    MOVE,
    SOLVEWAYPOINT,
    CHASE,
}

public class HeroStateMachine : MonoBehaviour
{
    [SerializeField] private HeroState _currentState;
    private WaypointsManager _wpManager;
    private EnemyMove _move;
    private Animator _animator;

    public HeroState CurrentState { get => _currentState; }

    private void Awake()
    {
        _move = GetComponent<EnemyMove>();
        _wpManager = FindObjectOfType<WaypointsManager>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // On initie la state machine
        OnStateEnter(HeroState.MOVE);
    }
    private void Update()
    {
        OnStateUpdate(_currentState);
    }

    #region STATE MACHINE PATTERN
    private void OnStateEnter(HeroState state)
    {
        switch (state)
        {
            case HeroState.MOVE:
                OnEnterMove();
                break;
            case HeroState.SOLVEWAYPOINT:
                OnEnterSolveWaypoint();
                break;
            case HeroState.CHASE:
                OnEnterChase();
                break;
            default:
                Debug.LogError("OnStateEnter: Invalid state " + state.ToString());
                break;
        }
    }
    private void OnStateUpdate(HeroState state)
    {
        switch (state)
        {
            case HeroState.MOVE:
                OnUpdateMove();
                break;
            case HeroState.SOLVEWAYPOINT:
                OnUpdateSolveWaypoint();
                break;
            case HeroState.CHASE:
                OnUpdateChase();
                break;
            default:
                Debug.LogError("OnStateUpdate: Invalid state " + state.ToString());
                break;
        }
    }
    private void OnStateExit(HeroState state)
    {
        switch (state)
        {
            case HeroState.MOVE:
                OnExitMove();
                break;
            case HeroState.SOLVEWAYPOINT:
                OnExitSolveWaypoint();
                break;
            case HeroState.CHASE:
                OnExitChase();
                break;
            default:
                Debug.LogError("OnStateExit: Invalid state " + state.ToString());
                break;
        }
    }
    public void TransitionToState(HeroState toState)
    {
        OnStateExit(_currentState);
        _currentState = toState;
        OnStateEnter(toState);
    }

    #endregion

    #region MOVE
    private void OnEnterMove()
    {
        _wpManager.StartMovingToNextWP();
    }
    private void OnUpdateMove()
    {
        if (_wpManager.IsSolving)
        {
            TransitionToState(HeroState.SOLVEWAYPOINT);
        }
        else if (_wpManager.IsBlocked)
        {
            // AJouter un state Perdu si besoin
        }
    }
    private void OnExitMove()
    {
    }
    #endregion

    #region SOLVEWAYPOINT
    private void OnEnterSolveWaypoint()
    {
        _move.StopAgent();
    }
    private void OnUpdateSolveWaypoint()
    {
        if (!_wpManager.IsSolving)
        {
            TransitionToState(HeroState.MOVE);
        }
    }
    private void OnExitSolveWaypoint()
    {
        
    }
    #endregion

    #region CHASE
    private void OnEnterChase()
    {
    }
    private void OnUpdateChase()
    {
    }
    private void OnExitChase()
    {
    }
    #endregion
}

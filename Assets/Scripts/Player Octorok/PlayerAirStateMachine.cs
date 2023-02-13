using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AirState
{
    OUT,
    SUB,
    OUTLIFT,
}

public class PlayerAirStateMachine : MonoBehaviour
{
    [SerializeField] private AirState _currentState;
    private PlayerInput _playerInput;
    private PlayerController _playerController;
    private LiftChecker _liftChecker;
    private Animator _animator;

    //private Rigidbody _rb;

    public AirState CurrentState { get => _currentState; }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerController = GetComponent<PlayerController>();
        //_rb = GetComponent<Rigidbody>();
        _liftChecker = GetComponentInChildren<LiftChecker>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        OnStateEnter(AirState.OUT);
    }
    private void Update()
    {
        OnStateUpdate(_currentState);
    }

    #region STATE MACHINE PATTERN
    private void OnStateEnter(AirState state)
    {
        switch (state)
        {
            case AirState.OUT:
                OnEnterOut();
                break;
            case AirState.SUB:
                OnEnterSub();
                break;
            case AirState.OUTLIFT:
                OnEnterOutLift();
                break;
            default:
                Debug.LogError("OnStateEnter: Invalid state " + state.ToString());
                break;
        }
    }
    private void OnStateUpdate(AirState state)
    {
        switch (state)
        {
            case AirState.OUT:
                OnUpdateOut();
                break;
            case AirState.SUB:
                OnUpdateSub();
                break;
            case AirState.OUTLIFT:
                OnUpdateOutLift();
                break;
            default:
                Debug.LogError("OnStateUpdate: Invalid state " + state.ToString());
                break;
        }
    }
    private void OnStateExit(AirState state)
    {
        switch (state)
        {
            case AirState.OUT:
                OnExitOut();
                break;
            case AirState.SUB:
                OnExitSub();
                break;
            case AirState.OUTLIFT:
                OnExitOutLift();
                break;
            default:
                Debug.LogError("OnStateExit: Invalid state " + state.ToString());
                break;
        }
    }
    public void TransitionToState(AirState toState)
    {
        OnStateExit(_currentState);
        _currentState = toState;
        OnStateEnter(toState);
    }

    #endregion

    #region OUT
    private void OnEnterOut()
    {
        
    }
    private void OnUpdateOut()
    {
        if (_playerInput.DigButton.IsDown)
        {
            TransitionToState(AirState.SUB);
        }
    }
    private void OnExitOut()
    {
    }
    #endregion

    #region SUB
    private void OnEnterSub()
    {
        _animator.SetTrigger("DigIn");
        _playerController.DigIn();
    }
    private void OnUpdateSub()
    {
        if (_playerInput.DigButton.IsDown)
        {
            // S'il y a un Liftable au-dessus au moment de sortir
            if (_liftChecker.HasSomethingToLift)
            {
                TransitionToState(AirState.OUTLIFT);
            }

            // S'il n'y a rien à porter au-dessus du Character
            else
            {
                TransitionToState(AirState.OUT);
            }
        }
    }
    private void OnExitSub()
    {
        _playerController.DigOut();
        _animator.SetTrigger("DigOut");
    }
    #endregion

    #region OUTLIFT
    private void OnEnterOutLift()
    {
        _animator.SetBool("Lift", true);
        _playerController.Lift(_liftChecker.Liftable);
    }
    private void OnUpdateOutLift()
    {
        if (_playerInput.DigButton.IsDown)
        {
            TransitionToState(AirState.SUB);
        }
    }
    private void OnExitOutLift()
    {
        _playerController.PutDown(_liftChecker.Liftable);
        _animator.SetBool("Lift", false);
        _liftChecker.ResetLiftable();
    }
    #endregion
}

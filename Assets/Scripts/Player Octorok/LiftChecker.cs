using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftChecker : MonoBehaviour
{
    [SerializeField] private bool _hasSomethingToLift;
    [SerializeField] private GameObject _liftable;

    [SerializeField] private PlayerAirStateMachine _airStateMachine;

    public bool HasSomethingToLift { get => _hasSomethingToLift; }
    public GameObject Liftable { get => _liftable; set => _liftable = value; }

    private void Awake()
    {
        _airStateMachine = transform.parent.GetComponent<PlayerAirStateMachine>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Liftable") && _liftable == null)
        {
            _hasSomethingToLift = true;
            _liftable = other.gameObject;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Liftable") && _liftable == null)
        {
            _hasSomethingToLift = true;
            _liftable = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if(other.gameObject == _liftable && _airStateMachine.CurrentState != AirState.OUTLIFT)
        if(other.gameObject == _liftable)
        {
            ResetLiftable();
        }
    }

    public void ResetLiftable()
    {
        _hasSomethingToLift = false;
        _liftable = null;
    }
}

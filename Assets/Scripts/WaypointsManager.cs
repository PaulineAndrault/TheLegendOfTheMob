using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointsManager : MonoBehaviour
{
    // Liste des WP
    [SerializeField] private List<Transform> _waypoints = new List<Transform>();

    // Propri�t�s priv�es
    private Transform _currentWaypoint;
    [SerializeField] private int _currentWpIndex = 0;  // Current waypoint index
    private bool _isSolving; // Pour interagir avec la state machine
    private bool _isBlocked; // Pour v�rifier si le h�ros a acc�s au WP suivant
    private float _timeBlocked = 0f;

    // R�f�rences
    private EnemyMove _move;
    private LevelManager _levelManager;

    // Propri�t�s publiques
    public Transform CurrentWaypoint { get => _waypoints[_currentWpIndex]; }
    public bool IsSolving { get => _isSolving; }
    public bool IsBlocked { get => _isBlocked; set => _isBlocked = value; }

    private void Awake()
    {
        _move = GameObject.FindGameObjectWithTag("Hero").GetComponent<EnemyMove>();
        _levelManager = GameObject.FindObjectOfType<LevelManager>();
    }

    private void LateUpdate()
    {
        // On ne fait plus rien s'il a perdu (isBlocked)
        if (_isBlocked) { return; }
        // On ne le fait que s'il est en phase MOVE (!isSolving)
        if (_isSolving) { return; }

        // On attend d'�tre s�r que le path est calcul�
        if(_move.Agent.pathPending || !_move.Agent.hasPath) { return; }

        Debug.Log(_move.Agent.pathStatus);
        Debug.Log(_timeBlocked);
        // Si le path n'est pas OK, on stoppe l'agent            
        if (_move.Agent.pathStatus == NavMeshPathStatus.PathPartial || _move.Agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            _timeBlocked += Time.deltaTime;
            if(_timeBlocked < 0.5f) { return; }
            _move.StopAgent();
            _isBlocked = true;
            _levelManager.PlayerWin();
            Debug.Log("Prochain WP inaccessible");
        }
        else if (_move.Agent.pathStatus == NavMeshPathStatus.PathComplete && _timeBlocked > 0f) { _timeBlocked = 0f; }
    }

    public void StartMovingToNextWP()
    {
        Debug.Log("Move to WP" + _currentWpIndex + "at " + Mathf.FloorToInt(_levelManager.CurrentTime / 60) + ":" + Mathf.FloorToInt(_levelManager.CurrentTime % 60));

        // On commence � suivre le nouveau WP
        _move.ChangeTarget(CurrentWaypoint);
        _move.StartAgent();
    }

    // Faire progresser l'index du Current Waypoint
    public void NextWaypoint()
    {
        // Bloquer s'il n'y a pas de waypoints
        if (_waypoints.Count == 0) { Debug.Log("Error");  return; }

        // Bloquer s'il n'y a plus d'autres wp
        if(_currentWpIndex >= _waypoints.Count - 1) { Debug.Log("Il n'y a plus de waypoints"); return; }

        // On d�finit la prochaine destination. Pas besoin de cycler ici.
        _currentWpIndex++;
    }
    public void ChooseNextWaypoint(int newWP)
    {
        _currentWpIndex = newWP;
    }

    // M�thode appel�e par WaypointEvents quand on entre dans une zone de WP � solver
    public void StartSolvingWaypoint()
    {
        Debug.Log("Start solving WP" + _currentWpIndex + "at " + Mathf.FloorToInt(_levelManager.CurrentTime / 60) + ":" + Mathf.FloorToInt(_levelManager.CurrentTime % 60));
        _isSolving = true;
    }

    // M�thode appel�e par WPEvents pour arr�ter de solver la zone
    public void StopSolvingWaypoint()
    {
        //Changer de state
        _isSolving = false;
    }
}

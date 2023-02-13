using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.AI;

public class WaypointEvents : MonoBehaviour
{
    // S�quence d'�venements
    [SerializeField] List<UnityEvent> _events = new List<UnityEvent>();
    
    // Variables de passage d'un event � l'autre
    private int _currentEvent = 0;
    private bool _eventDone = false;
    private bool _updatePause = false;
    private bool _lastEventSuccess = true;

    // Variables pour le NavMesh
    private bool _verifyPathTrigger;
    private Transform _currentTarget;
    private Vector3 _previousTargetPosition;

    // Variables de distances
    [Header("Action Go To")]
    [SerializeField] private float _stopNearTargetDistance = 2f;

    // Variables de Rubies
    [Header("Action Pick / Earn Rubies")]
    [SerializeField] private float _maxPickDistance;
    [SerializeField, Tooltip("Son de l'action Earn Rubies")] private AudioClip _rubyClip;

    // Variables d'entr�e / sortie de maison
    [Header("Entr�e / Sortie de maison")]
    [SerializeField] private AudioClip _enterHouseSound;
    [SerializeField] private AudioClip _exitHouseSound;

    // R�f�rences de composants
    private Transform _transform;
    private HeroStateMachine _stateMachine;
    private Transform _hero;
    private Rigidbody _rbHero;
    private Animator _animator;
    private EnemyMove _heroMove;
    private HeroInventory _inventory;
    private WaypointsManager _wpManager;
    private DialogueManager _dialogueManager;
    private LevelManager _levelManager;
    private AudioSource _heroAudioSource;

    private void Awake()
    {
        // On r�cup�re les r�f�rences
        _transform = GetComponent<Transform>();
        _hero = GameObject.FindGameObjectWithTag("Hero").GetComponent<Transform>();
        _rbHero = GameObject.FindGameObjectWithTag("Hero").GetComponent<Rigidbody>();
        _animator = GameObject.FindGameObjectWithTag("Hero").GetComponentInChildren<Animator>();
        _heroMove = GameObject.FindGameObjectWithTag("Hero").GetComponent<EnemyMove>();
        _heroAudioSource = GameObject.FindGameObjectWithTag("Hero").GetComponent<AudioSource>();
        _stateMachine = GameObject.FindGameObjectWithTag("Hero").GetComponent<HeroStateMachine>();
        _wpManager = FindObjectOfType<WaypointsManager>();
        _inventory = FindObjectOfType<HeroInventory>();
        _dialogueManager = FindObjectOfType<DialogueManager>();
        _levelManager = FindObjectOfType<LevelManager>();
    }

    private void Update()
    {
        // Si on ne souhaite pas d�clencher d'events, on bloque l'Update
        if (_updatePause) { return; }

        // Quand un event est fini, on lance l'event suivant
        if (_eventDone)
        {
            // On bloque l'Update en annon�ant qu'un event est en cours
            _eventDone = false;

            // Si tous les events ont eu lieu, on arr�te
            if(_currentEvent >= _events.Count) { return; }

            // Sinon, on appelle l'Event
            _events[_currentEvent].Invoke();

            // Puis on incr�mente le n� d'event
            _currentEvent++;
        }
    }

    private void LateUpdate()
    {
        // On attend d'�tre s�r que le path est calcul�
        if (_heroMove.Agent.pathPending || !_heroMove.Agent.hasPath) { return; }

        // Lors d'un GO TO, on contr�le que le Path n'est pas bloqu� en cours de d�placement
        if (_verifyPathTrigger && _currentTarget != null) { VerifyPath(); }

        // Lors d'un TAKE ITEM, on v�rifie que l'item n'a pas boug� en cours de d�placement

    }

    // On d�clenche la s�quence d'�v�nement quand le Hero trigger le Waypoint pendant une phase MOVE et qu'il s'agit bien du Waypoint cibl�
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hero") && _stateMachine.CurrentState == HeroState.MOVE && _wpManager.CurrentWaypoint == gameObject.transform)
        {
            // On fait �voluer le WaypointManager et la Hero State MAchine
            _wpManager.StartSolvingWaypoint();

            // On d�clenche le boolean qui va appeler le premier event de la s�quence
            _eventDone = true;
        }
    }

    #region DEPLACEMENT

    // D�placer le H�ros
    public void GoTo(Transform target)
    {
        // Si la target n'existe plus (exemple : pickable d�j� ramass�), alors on arr�te
        if (target == null) { _lastEventSuccess = false; _eventDone = true; return; }

        // On change la target du H�ros
        _heroMove.ChangeTarget(target);
        _currentTarget = target;
        _previousTargetPosition = target.position;

        // On commence la coroutine qui attend la fin de d�placement
        StartCoroutine(GoToCoroutine(target));
    }
    private IEnumerator GoToCoroutine(Transform target)
    {
        // On attend d'�tre s�r que le path est calcul�
        yield return new WaitUntil(() => !_heroMove.Agent.pathPending && _heroMove.Agent.hasPath);

        // Si la target n'est pas accessible, on d�clare le last Event Fail
        if (_heroMove.Agent.pathStatus == NavMeshPathStatus.PathPartial || _heroMove.Agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            _heroMove.StopAgent();
            _lastEventSuccess = false;
            _currentTarget = null;
            _eventDone = true;
            yield break;
        }
        // On relance le mouvement de l'Agent
        _heroMove.StartAgent();

        // On v�rifie ensuite r�guli�rement que le path n'a pas �t� bloqu� entre temps
        _verifyPathTrigger = true;

        // On attend que le H�ros soit suffisamment proche de la fin du path
        yield return new WaitUntil(() => Vector3.Distance(target.position, _hero.position) < _stopNearTargetDistance);
        
        // On arr�te l'Agent
        _heroMove.StopAgent();
        _verifyPathTrigger = false;
        _currentTarget = null;

        // Event termin�, on passe au suivant
        _lastEventSuccess = true;
        _eventDone = true;
    }

    private void VerifyPath()
    {
        // Si le path n'est pas OK, on stoppe l'agent            
        if (_heroMove.Agent.pathStatus == NavMeshPathStatus.PathPartial || _heroMove.Agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            _heroMove.StopAgent();
            StopAllCoroutines();
            _verifyPathTrigger = false;
            _currentTarget = null;
            _lastEventSuccess = false;
            _eventDone = true;
        }

        // Si la target a boug�, on recalcule le path
        if(Vector3.Distance(_currentTarget.position, _previousTargetPosition) > 1f)
        {
            Debug.Log("L'objet a boug�");
            _heroMove.ChangeTarget(_currentTarget);
            _heroMove.StartAgent();
        }
    }

    // Orienter le H�ros
    public void LookAt(Transform target)
    {
        _rbHero.velocity = Vector3.zero;
        _rbHero.angularVelocity = Vector3.zero;
        if(target != null) { _hero.LookAt(target); _lastEventSuccess = true; }
        _eventDone = true;
    }

    public void EnterHouse()
    {
        // Lancer une animation o� Link disparait en avan�ant
        // Mais son transform parent ne bouge pas.
        _animator.SetTrigger("EnterHouse");
        _heroAudioSource.clip = _enterHouseSound;
        _heroAudioSource.PlayDelayed(0.5f);
        _lastEventSuccess = true;
        _eventDone = true;
    }
    public void ExitHouse()
    {
        // Lancer une animation o� Link disparait en avan�ant
        // Mais son transform parent ne bouge pas.
        _animator.SetTrigger("ExitHouse");
        _heroAudioSource.PlayOneShot(_exitHouseSound);
        _lastEventSuccess = true;
        _eventDone = true;
    }

    #endregion

    #region PARLER / LIRE

    // D�clencher un dialogue
    public void TalkTo(TargetPNJ target)
    {
        // Si le PNJ est d�truit ou en fuite, l'action �choue
        if (target == null || target.CurrentPnjState == PNJState.FLEEING) { _lastEventSuccess = false; _eventDone = true; return; }

        // On v�rifie si un BARK est en cours
        if (target.CurrentPnjState == PNJState.BARKING)
        {
            // Si oui, on arr�te le Bark avant de lancer le Talk
            target.StopBarks();
        }

        StartCoroutine(TalkToCoroutine(target));
    }
    private IEnumerator TalkToCoroutine(TargetPNJ target)
    {
        //yield return new WaitForSeconds(0.2f);
        yield return null;

        target.StartTalk();
        // On attend que les coroutines Type() du Dialog Manager se soient toutes d�roul�es
        // OR FLEEING ??????
        yield return new WaitUntil(() => target.CurrentPnjState == PNJState.IDLE);
        _lastEventSuccess = true;
        _eventDone = true;
    }

    public void ReadSign(TargetSign target)
    {
        // Si le PNJ est d�truit ou en fuite, l'action �choue
        if (target == null) { _lastEventSuccess = false; _eventDone = true; return; }

        StartCoroutine(ReadSignCoroutine(target));
    }

    private IEnumerator ReadSignCoroutine(TargetSign target)
    {
        yield return null;

        target.StartTalk();
        yield return new WaitUntil(() => target.TalkEnded);
        _lastEventSuccess = true;
        _eventDone = true;
    }
    #endregion

    #region GAGNER QQCH

    // Ramasser un objet
    public void Take(GameObject pickable)
    {
        pickable.GetComponent<Pickable>().TakenByHero();
        _lastEventSuccess = true;
        _eventDone = true;
    }

    // Vider un slot de l'inventaire
    public void ClearInventorySlot(Transform slot)
    {
        if (slot.childCount != 1) { _lastEventSuccess = false; return; }
        _inventory.ClearSlot(slot);
        _lastEventSuccess = true;
        _eventDone = true;
    }

    // Ajouter un objet � un slot de l'inventaire
    public void PutItemInSlot(GameObject itemInUI)
    {
        // S'il n'y a pas de slot disponible, l'action est un �chec
        if (!_inventory.HasAvailableSlot()) { _lastEventSuccess = false; }

        // On ajoute l'item
        else 
        {
            _inventory.EarnItem(itemInUI);
            _inventory.PutItemInAvailableSlot(itemInUI);
            _lastEventSuccess = true;
        }
        _eventDone = true;
    }

    // Ramasser les rubis autour
    public void PickRubies()
    {
        // On met l'update en pause pour pouvoir invoke une suite d'actions sans d�clencher la suite des WP events
        _updatePause = true;

        // On r�cup�re tous les Rubis du jeu
        Ruby[] allRubies = FindObjectsOfType<Ruby>();

        // On lance la coroutine qui les r�cup�re
        StartCoroutine(PickRubiesCoroutine(allRubies));
    }

    //private IEnumerator PickRubiesCoroutine(Ruby[] allRubies) 
    private IEnumerator PickRubiesCoroutine(Ruby[] allRubies)
    {
        // Pour chaque rubis, on regarde l'accessibilit� en v�rifiant la distance au WP 
        foreach (Ruby ruby in allRubies)
        {
            // On ne fait rien si le rubis a d�j� �t� ramass�
            if (ruby == null) { }

            else
            {
                if (Vector3.Distance(ruby.transform.position, _transform.position) <= _maxPickDistance)
                {
                    // On r�initialise le bool�an pour pouvoir suivre l'�tat du d�placement
                    _eventDone = false;
                    _lastEventSuccess = true;
                    // On d�place le h�ros vers le rubis
                    GoTo(ruby.transform);
                    // On attend la fin du d�placement
                    yield return new WaitUntil(() => _eventDone);
                    yield return new WaitForSeconds(0.2f);
                    // D�sormais, les rubis sont pris avec un OnTrigger, plus besoin de Take()
                }
            }
        }
        // Ramassage termin�
        _lastEventSuccess = true;
        _eventDone = true;
        // On r�active l'Update
        _updatePause = false;
    }

    // Ajouter des rubis � l'inventaire
    public void EarnRubies(int amount)
    {
        StartCoroutine(EarnMoneyCoroutine(amount));
    }
    private IEnumerator EarnMoneyCoroutine(int amount)
    {
        AudioSource soundSource = GameObject.Find("SoundSource").GetComponent<AudioSource>();

        // Cas o� le H�ros gagne des rubis
        if(amount >= 0)
        {
            for (int i = 0; i < amount; i++)
            {
                _inventory.EarnMoney(1);
                soundSource.PlayOneShot(_rubyClip);
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Cas o� il d�pense des rubis
        else
        {
            for (int i = 0; i > amount; i--)
            {
                _inventory.EarnMoney(-1);
                soundSource.PlayOneShot(_rubyClip);
                yield return new WaitForSeconds(0.1f);
            }
        }
        _lastEventSuccess = true;
        _eventDone = true;
    }

    // Ouvrir un coffre
    public void OpenChest(GameObject chest)
    {

    }

    public void HasEnoughRubies(int minAmount)
    {
        _lastEventSuccess = _inventory.TotalRubies.Value < minAmount ? false : true;
        _eventDone = true;
    }

    public void ItemIsInArea(Transform item)
    {
        _lastEventSuccess = Vector3.Distance(item.position, _hero.position) >= _maxPickDistance ? false : true;
        _eventDone = true;
    }

    #endregion

    #region METHODES DE SEQUENCE

    // Cas o� une �tape du WP saute, sans cons�quences sur la s�quence
    public void SkipXStepsIfLastEventFailed(int numberOfStepsToSkip)
    {
        if (!_lastEventSuccess)
        {
            _currentEvent += numberOfStepsToSkip;
            _lastEventSuccess = true;
        }

        _eventDone = true;
    }

    // Cas o� tout le reste du WP saute, avec "lastEvent" qui reste FAIL. Id�al pour finir sur un "ChangeSequence..."
    public void SkipEndOfWPIfLastEventFail()
    {
        if (!_lastEventSuccess && _currentEvent < _events.Count - 2)
        {
            _currentEvent = _events.Count - 2;
            _lastEventSuccess = true;
        }
        _eventDone = true;
    }

    // Changer le d�roul� des s�quences
    public void ChangeSequenceIfLastEventFail(int nextWP)
    {
        if (!_lastEventSuccess)
        {
            _wpManager.ChooseNextWaypoint(nextWP);
            _lastEventSuccess = true;
        }
        _eventDone = true;
    }

    // Attendre X secondes avant l'event suivant
    public void Wait(float sec)
    {
        StartCoroutine(WaitCoroutine(sec));
    }

    public IEnumerator WaitCoroutine(float sec)
    {
        yield return new WaitForSeconds(sec);
        _lastEventSuccess = true;
        _eventDone = true;
    }

    // Arr�ter de solve ce WP et repartir vers un le next WP
    public void StopSolving()
    {
        _wpManager.NextWaypoint();
        _wpManager.StopSolvingWaypoint();
    }

    // Changer la s�quence en cas de r�ussite du WP
    public void StopSolvingAndChangeSeq(int nextWP)
    {
        _wpManager.ChooseNextWaypoint(nextWP);
        _wpManager.StopSolvingWaypoint();
    }

    public void HeroLose()
    {
        Debug.Log("Le H�ros a perdu");
        _levelManager.PlayerWin();
    }

    public void HeroWin()
    {
        Debug.Log("Le H�ros a gagn�");
        _levelManager.HeroWin();
    }

    #endregion

    #region GIZMOS
    public void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, Vector3.forward * _maxPickDistance, Color.red);
        Debug.DrawRay(transform.position, Vector3.back * _maxPickDistance, Color.red);
        Debug.DrawRay(transform.position, Vector3.left * _maxPickDistance, Color.red);
        Debug.DrawRay(transform.position, Vector3.right * _maxPickDistance, Color.red);
        Debug.DrawRay(transform.position, (Vector3.right + Vector3.forward).normalized * _maxPickDistance, Color.red);
        Debug.DrawRay(transform.position, (Vector3.right + Vector3.back).normalized * _maxPickDistance, Color.red);
        Debug.DrawRay(transform.position, (Vector3.left + Vector3.forward).normalized * _maxPickDistance, Color.red);
        Debug.DrawRay(transform.position, (Vector3.left + Vector3.back).normalized * _maxPickDistance, Color.red);
    }
    #endregion

}

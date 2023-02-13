using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.AI;

public class WaypointEvents : MonoBehaviour
{
    // Séquence d'évenements
    [SerializeField] List<UnityEvent> _events = new List<UnityEvent>();
    
    // Variables de passage d'un event à l'autre
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

    // Variables d'entrée / sortie de maison
    [Header("Entrée / Sortie de maison")]
    [SerializeField] private AudioClip _enterHouseSound;
    [SerializeField] private AudioClip _exitHouseSound;

    // Références de composants
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
        // On récupère les références
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
        // Si on ne souhaite pas déclencher d'events, on bloque l'Update
        if (_updatePause) { return; }

        // Quand un event est fini, on lance l'event suivant
        if (_eventDone)
        {
            // On bloque l'Update en annonçant qu'un event est en cours
            _eventDone = false;

            // Si tous les events ont eu lieu, on arrête
            if(_currentEvent >= _events.Count) { return; }

            // Sinon, on appelle l'Event
            _events[_currentEvent].Invoke();

            // Puis on incrémente le n° d'event
            _currentEvent++;
        }
    }

    private void LateUpdate()
    {
        // On attend d'être sûr que le path est calculé
        if (_heroMove.Agent.pathPending || !_heroMove.Agent.hasPath) { return; }

        // Lors d'un GO TO, on contrôle que le Path n'est pas bloqué en cours de déplacement
        if (_verifyPathTrigger && _currentTarget != null) { VerifyPath(); }

        // Lors d'un TAKE ITEM, on vérifie que l'item n'a pas bougé en cours de déplacement

    }

    // On déclenche la séquence d'événement quand le Hero trigger le Waypoint pendant une phase MOVE et qu'il s'agit bien du Waypoint ciblé
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hero") && _stateMachine.CurrentState == HeroState.MOVE && _wpManager.CurrentWaypoint == gameObject.transform)
        {
            // On fait évoluer le WaypointManager et la Hero State MAchine
            _wpManager.StartSolvingWaypoint();

            // On déclenche le boolean qui va appeler le premier event de la séquence
            _eventDone = true;
        }
    }

    #region DEPLACEMENT

    // Déplacer le Héros
    public void GoTo(Transform target)
    {
        // Si la target n'existe plus (exemple : pickable déjà ramassé), alors on arrête
        if (target == null) { _lastEventSuccess = false; _eventDone = true; return; }

        // On change la target du Héros
        _heroMove.ChangeTarget(target);
        _currentTarget = target;
        _previousTargetPosition = target.position;

        // On commence la coroutine qui attend la fin de déplacement
        StartCoroutine(GoToCoroutine(target));
    }
    private IEnumerator GoToCoroutine(Transform target)
    {
        // On attend d'être sûr que le path est calculé
        yield return new WaitUntil(() => !_heroMove.Agent.pathPending && _heroMove.Agent.hasPath);

        // Si la target n'est pas accessible, on déclare le last Event Fail
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

        // On vérifie ensuite régulièrement que le path n'a pas été bloqué entre temps
        _verifyPathTrigger = true;

        // On attend que le Héros soit suffisamment proche de la fin du path
        yield return new WaitUntil(() => Vector3.Distance(target.position, _hero.position) < _stopNearTargetDistance);
        
        // On arrête l'Agent
        _heroMove.StopAgent();
        _verifyPathTrigger = false;
        _currentTarget = null;

        // Event terminé, on passe au suivant
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

        // Si la target a bougé, on recalcule le path
        if(Vector3.Distance(_currentTarget.position, _previousTargetPosition) > 1f)
        {
            Debug.Log("L'objet a bougé");
            _heroMove.ChangeTarget(_currentTarget);
            _heroMove.StartAgent();
        }
    }

    // Orienter le Héros
    public void LookAt(Transform target)
    {
        _rbHero.velocity = Vector3.zero;
        _rbHero.angularVelocity = Vector3.zero;
        if(target != null) { _hero.LookAt(target); _lastEventSuccess = true; }
        _eventDone = true;
    }

    public void EnterHouse()
    {
        // Lancer une animation où Link disparait en avançant
        // Mais son transform parent ne bouge pas.
        _animator.SetTrigger("EnterHouse");
        _heroAudioSource.clip = _enterHouseSound;
        _heroAudioSource.PlayDelayed(0.5f);
        _lastEventSuccess = true;
        _eventDone = true;
    }
    public void ExitHouse()
    {
        // Lancer une animation où Link disparait en avançant
        // Mais son transform parent ne bouge pas.
        _animator.SetTrigger("ExitHouse");
        _heroAudioSource.PlayOneShot(_exitHouseSound);
        _lastEventSuccess = true;
        _eventDone = true;
    }

    #endregion

    #region PARLER / LIRE

    // Déclencher un dialogue
    public void TalkTo(TargetPNJ target)
    {
        // Si le PNJ est détruit ou en fuite, l'action échoue
        if (target == null || target.CurrentPnjState == PNJState.FLEEING) { _lastEventSuccess = false; _eventDone = true; return; }

        // On vérifie si un BARK est en cours
        if (target.CurrentPnjState == PNJState.BARKING)
        {
            // Si oui, on arrête le Bark avant de lancer le Talk
            target.StopBarks();
        }

        StartCoroutine(TalkToCoroutine(target));
    }
    private IEnumerator TalkToCoroutine(TargetPNJ target)
    {
        //yield return new WaitForSeconds(0.2f);
        yield return null;

        target.StartTalk();
        // On attend que les coroutines Type() du Dialog Manager se soient toutes déroulées
        // OR FLEEING ??????
        yield return new WaitUntil(() => target.CurrentPnjState == PNJState.IDLE);
        _lastEventSuccess = true;
        _eventDone = true;
    }

    public void ReadSign(TargetSign target)
    {
        // Si le PNJ est détruit ou en fuite, l'action échoue
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

    // Ajouter un objet à un slot de l'inventaire
    public void PutItemInSlot(GameObject itemInUI)
    {
        // S'il n'y a pas de slot disponible, l'action est un échec
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
        // On met l'update en pause pour pouvoir invoke une suite d'actions sans déclencher la suite des WP events
        _updatePause = true;

        // On récupère tous les Rubis du jeu
        Ruby[] allRubies = FindObjectsOfType<Ruby>();

        // On lance la coroutine qui les récupère
        StartCoroutine(PickRubiesCoroutine(allRubies));
    }

    //private IEnumerator PickRubiesCoroutine(Ruby[] allRubies) 
    private IEnumerator PickRubiesCoroutine(Ruby[] allRubies)
    {
        // Pour chaque rubis, on regarde l'accessibilité en vérifiant la distance au WP 
        foreach (Ruby ruby in allRubies)
        {
            // On ne fait rien si le rubis a déjà été ramassé
            if (ruby == null) { }

            else
            {
                if (Vector3.Distance(ruby.transform.position, _transform.position) <= _maxPickDistance)
                {
                    // On réinitialise le booléan pour pouvoir suivre l'état du déplacement
                    _eventDone = false;
                    _lastEventSuccess = true;
                    // On déplace le héros vers le rubis
                    GoTo(ruby.transform);
                    // On attend la fin du déplacement
                    yield return new WaitUntil(() => _eventDone);
                    yield return new WaitForSeconds(0.2f);
                    // Désormais, les rubis sont pris avec un OnTrigger, plus besoin de Take()
                }
            }
        }
        // Ramassage terminé
        _lastEventSuccess = true;
        _eventDone = true;
        // On réactive l'Update
        _updatePause = false;
    }

    // Ajouter des rubis à l'inventaire
    public void EarnRubies(int amount)
    {
        StartCoroutine(EarnMoneyCoroutine(amount));
    }
    private IEnumerator EarnMoneyCoroutine(int amount)
    {
        AudioSource soundSource = GameObject.Find("SoundSource").GetComponent<AudioSource>();

        // Cas où le Héros gagne des rubis
        if(amount >= 0)
        {
            for (int i = 0; i < amount; i++)
            {
                _inventory.EarnMoney(1);
                soundSource.PlayOneShot(_rubyClip);
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Cas où il dépense des rubis
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

    // Cas où une étape du WP saute, sans conséquences sur la séquence
    public void SkipXStepsIfLastEventFailed(int numberOfStepsToSkip)
    {
        if (!_lastEventSuccess)
        {
            _currentEvent += numberOfStepsToSkip;
            _lastEventSuccess = true;
        }

        _eventDone = true;
    }

    // Cas où tout le reste du WP saute, avec "lastEvent" qui reste FAIL. Idéal pour finir sur un "ChangeSequence..."
    public void SkipEndOfWPIfLastEventFail()
    {
        if (!_lastEventSuccess && _currentEvent < _events.Count - 2)
        {
            _currentEvent = _events.Count - 2;
            _lastEventSuccess = true;
        }
        _eventDone = true;
    }

    // Changer le déroulé des séquences
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

    // Arrêter de solve ce WP et repartir vers un le next WP
    public void StopSolving()
    {
        _wpManager.NextWaypoint();
        _wpManager.StopSolvingWaypoint();
    }

    // Changer la séquence en cas de réussite du WP
    public void StopSolvingAndChangeSeq(int nextWP)
    {
        _wpManager.ChooseNextWaypoint(nextWP);
        _wpManager.StopSolvingWaypoint();
    }

    public void HeroLose()
    {
        Debug.Log("Le Héros a perdu");
        _levelManager.PlayerWin();
    }

    public void HeroWin()
    {
        Debug.Log("Le Héros a gagné");
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

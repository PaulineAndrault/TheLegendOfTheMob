using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PNJState
{
    IDLE,
    TALKING,
    BARKING,
    FLEEING,
}

public class TargetPNJ : MonoBehaviour
{
    // Variable de state du PNJ
    [SerializeField] private PNJState _currentPnjState;
    private bool _hasBarked; // Pour empêcher le bark de boucler tant que le joueur n'est pas ressorti de la zone de détection
    private bool _hasFled; // Pour empêcher le bark de boucler tant que le joueur n'est pas ressorti de la zone de détection

    // Paramètres de Dialogue
    [SerializeField, Tooltip("Dialogue avec le Héros")] private string[] _sentences;
    [SerializeField, Tooltip("Barks entendus par le Player")] private string[] _barks;
    [SerializeField, Tooltip("Dialogue quand le PNJ fuit")] private string[] _fleeSentences;
    
    // Propriétés de détection du Player
    [Header("Détection du Player")]
    [SerializeField] private float _detectionRadius;
    [SerializeField] private LayerMask _playerOutLayer;
    [SerializeField] private LayerMask _playerSubLayer;

    // Paramètres d'UI
    [Header("Bulles de discussion")]
    [SerializeField] private GameObject _pTalkBubble;
    [SerializeField] private GameObject _pFleeBubble;
    private GameObject _currentBubble;

    // Paramètres de son
    [Header("Sounds")]
    private AudioSource _audio;
    [SerializeField] private AudioClip _talkSound;
    [SerializeField] private AudioClip _fleeSound;

    // Références
    private Transform _transform;
    private Transform _heroTransform;
    private Transform _playerTransform;
    private Transform _worldspaceCanvas;
    private DialogueManager _dialogueManagerHero;
    private DialogueManager _dialogueManagerPlayer;

    // Propriétés publiques
    public string[] Sentences { get => _sentences; set => _sentences = value; }
    public PNJState CurrentPnjState { get => _currentPnjState; set => _currentPnjState = value; }

    private void Awake()
    {
        _transform = transform;
        _heroTransform = GameObject.FindGameObjectWithTag("Hero").GetComponent<Transform>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        _worldspaceCanvas = GameObject.Find("WorldspaceCanvas").GetComponent<Transform>();
        _dialogueManagerHero = GameObject.Find("DialogueManagerHero").GetComponent<DialogueManager>();
        _dialogueManagerPlayer = GameObject.Find("DialogueManagerPlayer").GetComponent<DialogueManager>();
        _audio = GetComponent<AudioSource>();
        _currentPnjState = PNJState.IDLE;
    }

    private void Update()
    {
        // A partir du moment où le PNJ fuit, l'Update est bloqué
        //if (_hasFled) { return; }
        CheckPlayerPresence();
    }

    private void CheckPlayerPresence()
    {
        if(Physics.CheckSphere(_transform.position, _detectionRadius, _playerOutLayer))
        {
            // Pour le moment, on ne gère pas le cas où le Player arrive pendant un Talk avec le Héros
            if(_currentPnjState != PNJState.TALKING && !_hasFled)
            {
                _hasFled = true;
                Flee();
            }
        }

        // On vérifie si le Player est sous terre et si le PNJ n'est pas en train de Bark ou de Talk, et s'il ne vient pas juste de Bark (_hasBarked)
        else if (Physics.CheckSphere(_transform.position, _detectionRadius, _playerSubLayer))
        {
            if (CurrentPnjState == PNJState.IDLE && !_hasBarked && !_hasFled)
            {
                Bark();
            }
        }

        // Si le joueur sort de la zone de détection, on remet à zéro les booléens
        else
        {
            // Si le joueur sort de la zone pendant un bark
            if (CurrentPnjState == PNJState.BARKING) 
            { 
                StopBarks(); 
            }
            else if(CurrentPnjState == PNJState.FLEEING)
            {
                StopFlee();
            }
            // On remet le bool de bloquage de bark à zéro
            _hasBarked = false;
        }
    }

    // Se déclenche quand le Player est repéré par le PNJ
    private void Flee()
    {
        // On oriente le PNJ vers le Player
        _transform.LookAt(_playerTransform);

        // Cri du PNJ
        if (_audio != null && _fleeSound != null) { _audio.PlayOneShot(_fleeSound); }
        
        // Si le PNJ était en train de Bark, arrêter le Bark
        if(_currentPnjState == PNJState.BARKING)
        {
            StopBarks();
        }


        // On instantie une bulle UI
        _currentBubble = Instantiate(_pFleeBubble, _transform.position + Vector3.up * 3 + Vector3.right, _pFleeBubble.transform.rotation, _worldspaceCanvas);

        // On empêche le Héros de parler au PNJ et on arrête l'Update
        _currentPnjState = PNJState.FLEEING;
        Debug.Log("On repasse en Flee (Flee)");

        // On lance le dialogue de fuite
        _dialogueManagerPlayer.StartDialog(_fleeSentences);
        
        // On lance le timer pour la disparition du PNJ
        StartCoroutine(FleeCoroutine());
    }

    private IEnumerator FleeCoroutine()
    {
        yield return new WaitUntil(() => _dialogueManagerPlayer.HasEnded);
        Destroy(_currentBubble);
        Destroy(gameObject, 1f);
    }

    public void StopFlee()
    {
        _dialogueManagerPlayer.EndDialog();
        Destroy(_currentBubble);
    }

    private void Bark()
    {
        // On coupe l'Update le temps du talk
        _currentPnjState = PNJState.BARKING;

        // On bloque le bark tant que le Player n'est pas ressorti de la zone de détection
        _hasBarked = true;

        // On instantie une bulle UI
        _currentBubble = Instantiate(_pTalkBubble, _transform.position + Vector3.up * 3 + Vector3.right, _pTalkBubble.transform.rotation, _worldspaceCanvas);

        // On joue le son du Talk
        if (_audio != null && _talkSound != null) { _audio.PlayOneShot(_talkSound); }

        // On lance le dialogue de bark
        _dialogueManagerPlayer.StartDialog(_barks);

        // On lance la coroutine qui attend la fin du bark
        StartCoroutine(BarkCoroutine());
    }

    private IEnumerator BarkCoroutine()
    {
        yield return new WaitUntil(() => _dialogueManagerPlayer.HasEnded);
        Destroy(_currentBubble);
        _currentPnjState = PNJState.IDLE;
        Debug.Log("On repasse en Idle (Bark Coroutine)");
    }

    public void StopBarks()
    {
        StopAllCoroutines();
        _dialogueManagerPlayer.EndDialog();
        Destroy(_currentBubble);
        _currentPnjState = PNJState.IDLE;
        Debug.Log("On repasse en Idle (StopBarks)");
    }

    public void StartTalk()
    {
        // On oriente le PNJ vers le Héros
        _transform.LookAt(_heroTransform);

        _currentPnjState = PNJState.TALKING;

        // On joue le son du talk
        if (_audio != null && _talkSound != null) { _audio.PlayOneShot(_talkSound); }

        // On instantie une bulle UI
        _currentBubble = Instantiate(_pTalkBubble, _transform.position + Vector3.up * 3 + Vector3.right, _pTalkBubble.transform.rotation, _worldspaceCanvas);

        _dialogueManagerHero.StartDialog(_sentences);
        StartCoroutine(TalkCoroutine());
    }

    private IEnumerator TalkCoroutine()
    {
        yield return new WaitUntil(() => _currentPnjState == PNJState.TALKING && _dialogueManagerHero.HasEnded);
        Destroy(_currentBubble);
        _currentPnjState = PNJState.IDLE;
    }

    public void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, Vector3.forward * _detectionRadius, Color.blue);
        Debug.DrawRay(transform.position, Vector3.back * _detectionRadius, Color.blue);
        Debug.DrawRay(transform.position, Vector3.left * _detectionRadius, Color.blue);
        Debug.DrawRay(transform.position, Vector3.right * _detectionRadius, Color.blue);
        Debug.DrawRay(transform.position, (Vector3.right + Vector3.forward).normalized * _detectionRadius, Color.blue);
        Debug.DrawRay(transform.position, (Vector3.right + Vector3.back).normalized * _detectionRadius, Color.blue);
        Debug.DrawRay(transform.position, (Vector3.left + Vector3.forward).normalized * _detectionRadius, Color.blue);
        Debug.DrawRay(transform.position, (Vector3.left + Vector3.back).normalized * _detectionRadius, Color.blue);
    }
}

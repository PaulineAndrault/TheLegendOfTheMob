using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSign : MonoBehaviour
{
    // Variable de state du PNJ
    private bool _hasBarked; // Pour emp�cher le bark de boucler tant que le joueur n'est pas ressorti de la zone de d�tection
    private bool _talkEnded;

    // Param�tres de Dialogue
    [SerializeField, Tooltip("Dialogue avec le H�ros")] private string[] _sentences;
    [SerializeField, Tooltip("Barks entendus par le Player")] private string[] _barks;
    [SerializeField] private AudioClip _talkSound;
    
    // Propri�t�s de d�tection du Player
    [SerializeField] private float _detectionRadius;
    [SerializeField] private LayerMask _playerOutLayer;

    // R�f�rences
    private Transform _transform;
    private DialogueManager _dialogueManagerHero;
    private DialogueManager _dialogueManagerPlayer;
    private AudioSource _audio;

    // Propri�t�s publiques
    public string[] Sentences { get => _sentences; set => _sentences = value; }
    public bool TalkEnded { get => _talkEnded; set => _talkEnded = value; }

    private void Awake()
    {
        _transform = transform;
        _dialogueManagerHero = GameObject.Find("DialogueManagerHero").GetComponent<DialogueManager>();
        _dialogueManagerPlayer = GameObject.Find("DialogueManagerPlayer").GetComponent<DialogueManager>();
        _audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckPlayerPresence();
    }

    private void CheckPlayerPresence()
    {
        // Le Player peut lire s'il est dehors
        if(Physics.CheckSphere(_transform.position, _detectionRadius, _playerOutLayer))
        {
            if (!_hasBarked)
            {
                Bark();
            }
        }

        // Si le joueur sort de la zone de d�tection, on remet � z�ro les bool�ens
        else 
        {
            if (_hasBarked)
            {
                StopBarks(); 
                _hasBarked = false;
            }
        }
    }

    private void Bark()
    {
        // On bloque le bark tant que le Player n'est pas ressorti de la zone de d�tection
        _hasBarked = true;

        // On joue le son
        if(_talkSound != null) { _audio.PlayOneShot(_talkSound); }

        // On lance le dialogue de bark
        _dialogueManagerPlayer.StartDialog(_barks);
    }

    public void StopBarks()
    {
        _dialogueManagerPlayer.EndDialog();
    }

    public void StartTalk()
    {
        _talkEnded = false;
        if(_talkSound != null) { _audio.PlayOneShot(_talkSound); }
        _dialogueManagerHero.StartDialog(_sentences);
        StartCoroutine(TalkCoroutine());
    }
    private IEnumerator TalkCoroutine()
    {
        yield return new WaitUntil(() => _dialogueManagerHero.HasEnded);
        _talkEnded = true;
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

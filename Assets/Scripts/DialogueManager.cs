using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // Propri�t�s priv�es
    private bool _hasEnded;

    // Param�tres des Dialog Boxes
    [Header("Dialog Box")]
    [SerializeField] private GameObject _dialogBox;
    [SerializeField] private TMP_Text _textContainer;
    
    // Param�tres de typing
    [Header("Param�tres de lecture du dialogue")]
    [SerializeField] private float _typingSpeed;
    [SerializeField, Tooltip("Temps laiss� pour la lecture d'une phrase")] private float _readingDelay;
    private string[] _sentences;
    private int _index = 0;

    // Variables d'audio et d'anim
    [SerializeField] private AudioClip _clip;
    private Animator _animator;
    private AudioSource _audioSource;

    // Propri�t�s publiques
    public bool HasEnded { get => _hasEnded; }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = _dialogBox.GetComponent<Animator>();
    }

    public void StartDialog(string[] sentences)
    {
        // On ne lance pas de dialogue s'il n'y a pas de phrases
        if(sentences.Length == 0) { return; }

        // Si un dialogue est d�j� en cours, couper l'ancien
        if (!HasEnded) { EndDialog(); }

        // On initialise les variables
        _sentences = sentences;
        _index = 0;
        _hasEnded = false;

        // On ouvre la dialog box correspondante
        _dialogBox.SetActive(true);

        // On lance l'anim d'ouverture de la box
        // On lance le son d'ouverture

        // On lance le typing
        StartCoroutine(Type());
    }

    public IEnumerator Type()
    {
        // On laisse un temps d'attente avant la premi�re phrase
        if(_index == 0) { yield return new WaitForSeconds(0.2f); }

        // On fait apparaitre la phrase active caract�re par caract�re
        foreach(char letter in _sentences[_index].ToCharArray())
        {
            _textContainer.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }

        // On laisse un temps de lecture
        yield return new WaitForSeconds(_readingDelay);

        // S'il reste des phrases, on lance la prochaine phrase
        if(_index < _sentences.Length - 1)
        {
            NextSentence();
        }

        // Si c'est la derni�re phrase, on arr�te l'action
        else
        {
            EndDialog();
        }
    }

    public void NextSentence()
    {
        if(_index < _sentences.Length - 1)
        {
            _index++;
            _textContainer.text = "";
            StartCoroutine(Type());
        }
    }

    public void EndDialog()
    {
        // En cas de End lanc� depuis un autre script, on arr�te les coroutines en cours
        if (!HasEnded) { StopAllCoroutines(); }
        
        _textContainer.text = "";
        // On lance l'anim de fermeture de la box
        // On lance le son de fermeture

        // On ferme la dialogBox
        _dialogBox.SetActive(false);

        // On modifie le bool�en HasEnded pour lancer la fin du Unity Event dans le WaypointEvents
        _hasEnded = true;
    }
}

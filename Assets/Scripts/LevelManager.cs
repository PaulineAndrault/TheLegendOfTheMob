using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Visuels")]
    [SerializeField] Animator _bgHero;
    [SerializeField] Animator _bgPlayer;
    [SerializeField] Animator _bgOverlay;
    [SerializeField] TMP_Text _finalText;
    [SerializeField] GameObject _button;

    [Header("Audio")]
    [SerializeField] AudioSource _music;
    [SerializeField] AudioSource _sound;
    [SerializeField] AudioClip _victorySound;
    [SerializeField] AudioClip _victoryMusic;
    [SerializeField] AudioClip _failureSound;
    [SerializeField] AudioClip _failureMusic;

    [Header("Dialogues")]
    [SerializeField] DialogueManager _dialManagerPlayer;
    [SerializeField] DialogueManager _dialManagerHero;
    [Header("If Player Loses")]
    [SerializeField] string[] _failureSentencesHero;
    [SerializeField] string[] _failureSentencesPlayer;
    [Header("If Player Wins")]
    [SerializeField] string[] _victorySentencesHero;
    [SerializeField] string[] _victorySentencesPlayer;

    // Timer
    private float _currentTime;
    private bool _timerIsOn;

    // Réfs
    private UIManager _ui;

    public float CurrentTime { get => _currentTime; }

    private void Awake()
    {
        _ui = FindObjectOfType<UIManager>();
        _currentTime = 0f;
        _timerIsOn = true;
    }

    private void Update()
    {
        if (!_timerIsOn) { return; }
        _currentTime += Time.deltaTime;
        _ui.RefreshTimer();
    }

    public void HeroWin()
    {
        _timerIsOn = false;
        StartCoroutine(HeroWinCoroutine());
    }
    public IEnumerator HeroWinCoroutine()
    {
        // L'écran du joueur se grise
        _bgPlayer.SetTrigger("FadeToBlack");
        //_bgPlayer.color = new Color(_bgPlayer.color.r, _bgPlayer.color.g, _bgPlayer.color.b, 0.6f);

        // Son / Musique d'échec
        _music.pitch = 0.6f;
        _sound.PlayOneShot(_failureSound);
        
        yield return new WaitForSeconds(2f);

        // Dialogue du Héros
        _dialManagerHero.StartDialog(_failureSentencesHero);
        yield return new WaitUntil(() => _dialManagerHero.HasEnded);

        _dialManagerPlayer.StartDialog(_failureSentencesPlayer);
        yield return new WaitUntil(() => _dialManagerPlayer.HasEnded);

        _bgOverlay.SetTrigger("FadeIn");

        yield return new WaitForSeconds(1);
        _music.Stop();
        _music.pitch = 1f;
        _music.PlayOneShot(_failureMusic);
        // UI Screenspace change texte
        _finalText.text = "Vous avez tenu " + string.Format("{0:00} min {1:00} sec.", Mathf.FloorToInt(CurrentTime / 60), Mathf.FloorToInt(CurrentTime % 60)) ;

        // Save score and send to google
        SaveScore("lose");
        FindObjectOfType<ScoreGoogle>().Send();

        // Bouton recommencer
        _button.SetActive(true);

    }

    public void PlayerWin()
    {
        _timerIsOn = false;
        StartCoroutine(PlayerWinCoroutine());
    }
    
    public IEnumerator PlayerWinCoroutine()
    {
        // L'écran du Héros se grise
        _bgHero.SetTrigger("FadeToBlack");
        //_bgHero.color = new Color(_bgHero.color.r, _bgHero.color.g, _bgHero.color.b, 0.6f);

        // Son / Musique de victoire
        _music.Stop();
        _sound.PlayOneShot(_victorySound);

        yield return new WaitForSeconds(2f);

        _music.clip = _victoryMusic;
        _music.Play();

        // Dialogue du Héros
        _dialManagerHero.StartDialog(_victorySentencesHero);
        yield return new WaitUntil(() => _dialManagerHero.HasEnded);

        _dialManagerPlayer.StartDialog(_victorySentencesPlayer);
        yield return new WaitUntil(() => _dialManagerPlayer.HasEnded);

        _bgOverlay.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1);

        // UI Screenspace change texte
        _finalText.text = "Mission accomplie en " + string.Format("{0:00} min {1:00}.", Mathf.FloorToInt(CurrentTime / 60), Mathf.FloorToInt(CurrentTime % 60));

        // Calcul et affiche le score
        SaveScore("win");
        FindObjectOfType<ScoreGoogle>().Send();

        // Bouton recommencer
        _button.SetActive(true);

    }

    private void SaveScore(string _winOrLose)
    {
        PlayerPrefs.SetString("Score", _winOrLose == "win" ? "a bloqué Link en..." : "a ralenti Link pendant...");
        PlayerPrefs.SetString("Time", string.Format("{0:00} min {1:00}", Mathf.FloorToInt(CurrentTime / 60), Mathf.FloorToInt(CurrentTime % 60)));
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

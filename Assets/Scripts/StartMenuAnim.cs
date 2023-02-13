using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StartMenuAnim : MonoBehaviour
{
    // Paramètres des Dialog Boxes
    [Header("MainTitle")]
    [SerializeField] private TMP_Text _textTitle;
    [SerializeField] private string _title = "The Legend of ";
    [Header("Zelda")]
    [SerializeField] private TMP_Text _textZelda;
    [SerializeField] private string _zelda = "Zelda";
    [Header("The Mob")]
    [SerializeField] private TMP_Text _textTheMob;
    [SerializeField] private string _theMob = "The Mob";
    [Header("Consignes")]
    [SerializeField] private TMP_Text _textConsignes;
    [SerializeField] private string _consignes = "Link still thinks he's the choosen one. You, a simple Octorock, has to prove him wrong.";

    [Header("Tuto")]
    [SerializeField] private GameObject _panneau;
    [SerializeField] private GameObject _panneau2;

    // Paramètres de typing
    [Header("Paramètres de lecture du dialogue")]
    [SerializeField] private float _typingSpeed;
    // [SerializeField] private AudioClip _clip;

    [SerializeField] Animator _uiAnimator;
    [SerializeField] AudioSource _music;
    [SerializeField] AudioSource _sound;
    [SerializeField] AudioClip _explosion;


    private void Start()
    {
        StartCoroutine(Type());
    }

    public IEnumerator Type()
    {
        yield return new WaitForSeconds(2f);

        // Ecrire The Legend Of
        foreach (char letter in _title.ToCharArray())
        {
            _textTitle.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }

        yield return new WaitForSeconds(0.5f);

        // Ecrire ZELDA
        foreach (char letter in _zelda.ToCharArray())
        {
            _textZelda.text += letter;
            yield return new WaitForSeconds(_typingSpeed * 5);
        }

        // Explosion et Effacer Zelda
        yield return new WaitForSeconds(1f);
        _sound.PlayOneShot(_explosion);
        yield return new WaitForSeconds(0.2f);

        foreach (char letter in _zelda.ToCharArray())
        {
            _zelda = _zelda.Substring(0, _zelda.Length - 1);
            Debug.Log(_zelda);
            _textZelda.text = _zelda;
            yield return new WaitForSeconds(_typingSpeed);
        }

        // Changer musique
        for (int i = 0; i < 40; i++)
        {
            _music.pitch -= 0.01f;
            yield return null;
        }

        yield return new WaitForSeconds(.5f);

        // Ecrire THE MOB
        foreach (char letter in _theMob.ToCharArray())
        {
            _textTheMob.text += letter;
            yield return new WaitForSeconds(_typingSpeed * 5);
        }

        yield return new WaitForSeconds(1f);

        // Animer le Titre puis afficher le sous-titre
        _uiAnimator.SetTrigger("TitleUp");

        yield return new WaitForSeconds(1.5f);

        foreach (char letter in _consignes.ToCharArray())
        {
            _textConsignes.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }

        // Transition vers tuto
        yield return new WaitForSeconds(1.5f);
        _uiAnimator.SetTrigger("EndTitle");

        yield return new WaitForSeconds(1.8f);

        // Premier dialogue tuto
        _panneau.SetActive(true);
        yield return new WaitForSeconds(25f);

        // Second dialogue tuto
        _panneau.SetActive(false);
        _panneau2.SetActive(true);

        yield return new WaitForSeconds(15f);
        SceneManager.LoadScene(2);
    }
}

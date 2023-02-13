using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    [SerializeField] protected int _value;

    // Audio
    [SerializeField] private AudioClip _clip;

    // Références
    protected HeroInventory _heroInventory;
    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        _heroInventory = FindObjectOfType<HeroInventory>();
        _animator = GetComponentInChildren<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public virtual void TakenByHero()
    {
        // La conséquence sur l'inventaire a lieu dans la classe fille (ex : Ruby)

        // On lance l'animation de disparition
        _animator.SetTrigger("Taken");

        // On joue le son de collecte
        if(_audioSource != null && _clip != null)
        {
            _audioSource.PlayOneShot(_clip);
        }

        // On détruit l'objet à la fin de l'anim
        Destroy(gameObject, 2.1f);
    }
}

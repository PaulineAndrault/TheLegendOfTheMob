using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AxisInput
{
    // Variables privées
    private Vector3 _value;

    // Constructeur
    public AxisInput(Vector3 vector)
    {
        // On clampe la magnitude à 1 max, pas besoin de normaliser ensuite.
        _value = Vector3.ClampMagnitude(vector, 1f);
    }

    // Propriétés
    public Vector3 Value { get => _value; }
    public bool HasMovement { get => _value != Vector3.zero; }
}

public struct ButtonInput
{
    // Variables privées
    private bool _isActive;
    private bool _isDown;
    private bool _isUp;

    // Constructeur
    public ButtonInput(bool isActive, bool isDown, bool isUp)
    {
        _isActive = isActive;
        _isDown = isDown;
        _isUp = isUp;
    }

    //Propriétés
    public bool IsActive { get => _isActive; }
    public bool IsDown { get => _isDown; }
    public bool IsUp { get => _isUp; }
}

public class PlayerInput : MonoBehaviour
{
    // Référence au Rigidbody du Character pour ne pas modifier son Y
    private Rigidbody _rb;

    // Référence à l'animator pour envoyer les infos de déplacements X / Z
    private Animator _animator;

    // Axes de déplacement
    private AxisInput _movement;

    // Boutons d'action
    private ButtonInput _digButton;

    // Propriétés publiques : on n'a besoin que des setters, car on va "get" les valeurs en Update
    public AxisInput Movement { get => _movement; }
    public ButtonInput DigButton { get => _digButton; }

    private void Awake()
    {
        // On récupère les références
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // On se déplace selon l'axe X et Z et on garde la valeur actuelle de Y du player intacte
        _movement = new AxisInput(new Vector3(Input.GetAxisRaw("Horizontal"), _rb.velocity.y, Input.GetAxisRaw("Vertical")));

        // On stocke la valeur de l'input Dig
        _digButton = new ButtonInput(Input.GetButton("Dig"), Input.GetButtonDown("Dig"), Input.GetButtonUp("Dig"));

        // On envoie les infos X / Z à l'animator
        _animator.SetFloat("XAxis", Input.GetAxisRaw("Horizontal"));
        _animator.SetFloat("ZAxis", Input.GetAxisRaw("Vertical"));
    }


}

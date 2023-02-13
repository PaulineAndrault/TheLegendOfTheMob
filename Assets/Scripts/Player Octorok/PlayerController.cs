using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    // Variables de déplacement
    [Header("Paramètres de vitesse de déplacement")]
    [SerializeField] private float _outSpeed = 2f;
    [SerializeField] private float _subSpeed = 5f;
    [SerializeField] private float _liftSpeed = 1f;

    // Sons liés au Player
    [Header("Player Sounds")]
    [SerializeField] private AudioClip _digInSound;
    [SerializeField] private AudioClip _digOutSound;
    [SerializeField] private AudioSource _footstepsAudioSource;
    [SerializeField] private AudioClip _stepsUnder;
    [SerializeField] private AudioClip _stepsOut;
    [SerializeField] private AudioSource _lifterAudioSource;
    [SerializeField] private AudioClip _liftSound;
    [SerializeField] private AudioClip _putDownSound;

    // Particules liées au Player
    [Header("Player Particles")]
    [SerializeField] private GameObject _undergroundStepsParticles;
    [SerializeField] private GameObject _outsideStepsParticles;
    [SerializeField] private GameObject _pPutDownParticles;
    [SerializeField] private GameObject _pDigOutParticles;

    // Variables de Lift
    private Transform _liftObjectParent;
    private float _yLiftObjectPosition;
    private float _xLiftObjectRotation;

    // Références composants
    private Rigidbody _rb;
    private Transform _tr;
    [SerializeField] private Transform _lifterTransform;
    private SortingGroup _sortingGroup;
    private Collider2D _col;
    private PlayerInput _input;
    private PlayerMoveStateMachine _moveStateMachine;
    private PlayerAirStateMachine _airStateMachine;
    private AudioSource _audioSource;

    private void Awake()
    {
        // On récupère les références
        _rb = GetComponent<Rigidbody>();
        _tr = GetComponent<Transform>();
        _sortingGroup = GetComponentInChildren<SortingGroup>();
        _col = GetComponent<Collider2D>();
        _input = GetComponent<PlayerInput>();
        _moveStateMachine = GetComponent<PlayerMoveStateMachine>();
        _airStateMachine = GetComponent<PlayerAirStateMachine>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }

    private void ApplyMovement()
    {
        // On détermine la vitesse selon le Current move State
        float speed;
        float footstepSpeed;
        switch (_airStateMachine.CurrentState)
        {
            case AirState.OUT:
                speed = _outSpeed;
                footstepSpeed = 1f;
                break;
            case AirState.SUB:
                speed = _subSpeed;
                footstepSpeed = 0.7f;
                break;
            case AirState.OUTLIFT:
                speed = _liftSpeed;
                footstepSpeed = 0.8f;
                break;
            default:
                speed = _outSpeed;
                footstepSpeed = 1f;
                break;
        }

        // On applique le mouvement
        _rb.velocity = _input.Movement.Value * speed;
        _footstepsAudioSource.pitch = _input.Movement.Value == Vector3.zero ? 0f : footstepSpeed;

    }

    private void ApplyRotation()
    {
        if (_input.Movement.Value.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(_input.Movement.Value);
            _rb.MoveRotation(lookRotation);
        }
    }

    public void DigIn()
    {
        // Changer de layer (pour les collisions)
        gameObject.layer = LayerMask.NameToLayer("SubPlayer");

        // Activer particules et son
        _outsideStepsParticles.SetActive(false);
        _undergroundStepsParticles.SetActive(true);
        _audioSource.PlayOneShot(_digInSound);
        _footstepsAudioSource.clip = _stepsUnder;
        _footstepsAudioSource.Play();

        // On active les particules de Dig (explosion)
        GameObject _particules = Instantiate(_pDigOutParticles, _tr.position, Quaternion.identity, null);
        Destroy(_particules, 3f);
    }

    public void DigOut()
    {
        // Changer de layer (pour les collisions)
        gameObject.layer = LayerMask.NameToLayer("Player");


        // Activer particules et son
        _undergroundStepsParticles.SetActive(false);
        _outsideStepsParticles.SetActive(true);
        _audioSource.PlayOneShot(_digOutSound);
        _footstepsAudioSource.clip = _stepsOut;
        _footstepsAudioSource.Play();

        // On active les particules de Dig (explosion)
        GameObject _particules = Instantiate(_pDigOutParticles, _tr.position, Quaternion.identity, null);
        Destroy(_particules, 3f);
    }

    public void Lift(GameObject liftedObject)
    {
        // On récupère et on enregistre le parent de l'objet lifté, sa position initiale sur le sol et sa rotation initiale sur l'axe X
        _liftObjectParent = liftedObject.transform.parent;
        _yLiftObjectPosition = liftedObject.transform.localPosition.y;
        _xLiftObjectRotation = liftedObject.transform.localEulerAngles.x;

        // On apparente l'objet lifté au Character
        liftedObject.transform.SetParent(_lifterTransform, false);

        // On joue le son du Lift
        _lifterAudioSource.PlayOneShot(_liftSound);

        // On positionne l'objet sur le dos du Character
        liftedObject.transform.localPosition = Vector3.zero;
        liftedObject.transform.localEulerAngles = new Vector3(liftedObject.GetComponent<Liftable>().XRotationWhenLifted, liftedObject.transform.localEulerAngles.y, liftedObject.transform.localEulerAngles.z);
    }

    public void PutDown(GameObject liftedObject)
    {
        // Cas où l'objet a été récupéré par le Héros sur le dos du Player
        if(liftedObject == null) { return; }

        // On re-parente l'objet à son parent initial
        liftedObject.transform.SetParent(_liftObjectParent);

        // On joue le son du Put Down
        _lifterAudioSource.PlayOneShot(_putDownSound);

        // On repositionne l'objet sur le sol
        liftedObject.transform.localPosition = new Vector3(liftedObject.transform.localPosition.x, _yLiftObjectPosition, liftedObject.transform.localPosition.z);
        liftedObject.transform.localEulerAngles = new Vector3(_xLiftObjectRotation, _tr.localEulerAngles.y, 0f);

        // On active les particules de Put Down
        GameObject _particules = Instantiate(_pPutDownParticles, liftedObject.transform);
        Destroy(_particules, 3f);
    }
}

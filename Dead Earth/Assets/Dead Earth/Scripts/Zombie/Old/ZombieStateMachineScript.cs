using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovementScript : MonoBehaviour
{
    #region Public
    public enum State
    {
        IDLE,
        MOVE,
    }
    #endregion

    #region Serialize Field
    [Header("Caractéristiques")]
    [Tooltip("Characterer controller du player.")]
    [SerializeField] CharacterController characterController = null;
    [Tooltip("Vitesse de marche du player.")]
    [SerializeField] float walkSpeed = 5f;
    [Tooltip("Vitesse de course du player.")]
    [SerializeField] float runSpeed = 10f;
    [Tooltip("Vitesse de rotation du player.")]
    [SerializeField] float rotateSpeed = 20f;
    [Tooltip("Hauteur du saut du player.")]
    [SerializeField] float jumpHeight = 3f;
    [Header("Ground check")]
    /*[Tooltip("Transform de l'empty servant de ground check.")]
    [SerializeField] Transform groundCheck = null;
    [Tooltip("Rayon de détection du ground check.")]
    [SerializeField] float groundDistance = 0.4f;
    [Tooltip("LayerMask sur lequel/lesquels le ground check sera effectué.")]
    [SerializeField] LayerMask groundMask = new LayerMask();*/
    [Tooltip("Rayon de détection du ground check.")]
    [SerializeField] float rayDistance = 1.3f;
    [Tooltip("LayerMask sur lequel/lesquels le ground check sera effectué.")]
    [SerializeField] LayerMask groundMask = new LayerMask();
    [Tooltip("Gravité.")]
    [SerializeField] float gravity = -9.81f;
    #endregion

    #region Private & Protected
    AudioSource _audioSource = null;
    Animator _animator = null;
    Vector3 _velocity = new Vector3(), _move = new Vector3(), _rotate = new Vector3();
    Dictionary<string, bool> _flags = new Dictionary<string, bool>();
    State _currentState = State.MOVE;
    RaycastHit _hit;
    float _actualSpeed = 0.0f, _xMove = 0.0f, _yMove = 0.0f;
    bool _isGrounded = true, _isIdle = true, _isWalking = false, _isRunning = false, _isTurning = false, _isStraffing = false;
    #endregion

    #region System
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _actualSpeed = walkSpeed;

        SetFlag("isIdle", _isIdle);
        SetFlag("isWalking", _isWalking);
        SetFlag("isRunning", _isRunning);
        SetFlag("isTurning", _isTurning);
        SetFlag("isStraffing", _isStraffing);
    }

    // Update is called once per frame
    void Update()
    {
        _xMove = Input.GetAxis("Horizontal");
        _yMove = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _actualSpeed = runSpeed;
            _xMove *= 2;
            _yMove *= 2;
        }
        else
        {
            _actualSpeed = walkSpeed;
        }

        SetFloatMove();

        switch (_currentState)
        {
            case State.IDLE:
                Idle();
                break;
            case State.MOVE:
                Move();
                Gravity();
                Jump();
                break;
            default:
                break;
        }


        characterController.Move(_move * walkSpeed * Time.deltaTime);
        transform.Rotate(_rotate * rotateSpeed * Time.deltaTime);
        characterController.Move(_velocity * Time.deltaTime);
    }
    #endregion

    #region Main Methods


    /*********************************************
    *  Méthodes des états
    *********************************************/
    void Idle()
    {
        TransitionToMove();
    }

    void Move()
    {
        _move = transform.forward * _yMove;
        _rotate = transform.up * _xMove;
    }

    /*********************************************
   *  Méthodes de transition des états
   *********************************************/

    void TransitionToIdle()
    {
        if (_xMove == 0 && _yMove == 0)
        {
            _currentState = State.IDLE;
        }
    }

    void TransitionToMove()
    {
        if ((_xMove != 0 || _yMove != 0))
        {
            _currentState = State.MOVE;
        }
    }
    /*********************************************
    *  Autres méthodes publiques
    *********************************************/

    /*********************************************
    *  Autres méthodes privées
    *********************************************/
    void SetFlag(string name, bool value)
    {
        _flags[name] = value;
        _animator.SetBool(name, value);
    }

    bool GetFlagByName(string name)
    {
        if (_flags.ContainsKey(name))
        {
            return _flags[name];
        }
        Debug.Log("Cet élément n'existe pas.");
        return false;
    }

    void SetFloatMove()
    {
        _animator.SetFloat("xMove", _xMove);
        _animator.SetFloat("yMove", _yMove);
    }

    void Gravity()
    {
        /* _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

         if (_isGrounded && _velocity.y < 0)
         {
             _velocity.y = -2f;
         }

         _velocity.y += gravity * Time.deltaTime;*/

        _isGrounded = Physics.Raycast(transform.position + characterController.center, Vector3.down, out _hit, rayDistance, groundMask);
        Debug.DrawRay(transform.position + characterController.center, transform.TransformDirection(Vector3.down) * rayDistance, Color.cyan);
        Debug.Log(_isGrounded);
        if (!_isGrounded)
        {
            _velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            _velocity.y = 0f;
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    #endregion   
}

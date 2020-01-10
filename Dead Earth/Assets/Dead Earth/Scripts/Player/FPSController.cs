using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#region Enum
public enum PlayerMoveStatus { NOT_MOVING, CROUCHING, WALKING, RUNNING, NOT_GROUNDED, LANDING }
public enum CurveControlledBobCallBackType { HORIZONTAL, VERTICAL }
#endregion

#region delegate
public delegate void CurveControlledBobCallBack();
#endregion

#region CurveControlledHeadBob
[Serializable]
public class CurveControlledHeadBob 
{
#region Serialize Field
    [Header("Paramètres du headbob.")]
    [Tooltip("Curve du temps.")]
    [SerializeField] AnimationCurve _bobCurve = new AnimationCurve( new Keyframe(0.0f,0.0f), new Keyframe(0.5f,1.0f),
                                                                    new Keyframe(1.0f,0.0f), new Keyframe(1.5f,-1.0f),
                                                                    new Keyframe(2.0f,0.0f) );
    [Tooltip("Valeur paramétrant la force du mouvement horizontal.")]
    [SerializeField] float _horizontalMultiplier = 0.01f;
    [Tooltip("Valeur paramétrant la force du mouvement vertical.")]
    [SerializeField] float _verticalMultiplier = 0.02f;
    [Tooltip("Valeur paramétrant la vitesse des mouvements.")]
    [SerializeField] float _verticalToHorizontalSpeedRatio = 2.0f;
    [Tooltip("Interval de base.")]
    [SerializeField] float _baseInterval = 1.0f;
#endregion

#region Private
    List<CurveControllerBobEvent> _events = new List<CurveControllerBobEvent>();
    float _prevXPlayHead, _prevYPlayHead, _xPlayHead, _yPlayHead, _curveEndTime; 
#endregion

#region Main Methods
//-------------------- Public -------------------- 
    public void Initialize()
    {
        _prevXPlayHead = 0.0f;
        _prevYPlayHead = 0.0f;
        _xPlayHead = 0.0f;
        _yPlayHead = 0.0f;
        _curveEndTime = _bobCurve[_bobCurve.length -1].time;
    }

    public void RegisterEventCallBack(float time, CurveControlledBobCallBack function, CurveControlledBobCallBackType type)
    {
        CurveControllerBobEvent ccbeEvent = new CurveControllerBobEvent();
        ccbeEvent.Time = time;
        ccbeEvent.Function = function;
        ccbeEvent.Type = type;
        _events.Add(ccbeEvent);

        _events.Sort(
            delegate (CurveControllerBobEvent t1, CurveControllerBobEvent t2)
            {
                return (t1.Time.CompareTo(t2.Time));
            }
        );
    }

    public Vector3 GetVectorOffset(float speed)
    {
        _xPlayHead += (speed * Time.deltaTime)/_baseInterval;
        _yPlayHead += (speed * Time.deltaTime)/_baseInterval *_verticalToHorizontalSpeedRatio;

        if(_xPlayHead > _curveEndTime)
        {
            _xPlayHead -= _curveEndTime;
        }

        if(_yPlayHead > _curveEndTime)
        {
            _yPlayHead -= _curveEndTime;
        }

        for(int i = 0; i < _events.Count; i++)
        {
            CurveControllerBobEvent ev = _events[i];

            if(ev != null)
            {
                if(ev.Type == CurveControlledBobCallBackType.VERTICAL)
                {
                    if((_prevYPlayHead < ev.Time && _yPlayHead >= ev.Time) || (_prevYPlayHead > _yPlayHead && (ev.Time > _prevYPlayHead || ev.Time <=  _yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                else if(ev.Type == CurveControlledBobCallBackType.HORIZONTAL)
                {
                    if((_prevXPlayHead < ev.Time && _xPlayHead >= ev.Time) || (_prevXPlayHead > _xPlayHead && (ev.Time > _prevXPlayHead || ev.Time <=  _xPlayHead)))
                    {
                        ev.Function();
                    }             
                }
            }
        }

        float xPos = _bobCurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobCurve.Evaluate(_yPlayHead) * _verticalMultiplier;

        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;

        return new Vector3(xPos, yPos, 0);
    }
#endregion
}
#endregion

#region CurveControllerBobEvent
[Serializable]
public class CurveControllerBobEvent
{
#region Public
    public float Time = 0.0f;
    public CurveControlledBobCallBack Function = null;
    public CurveControlledBobCallBackType Type = CurveControlledBobCallBackType.VERTICAL;
#endregion
}
#endregion

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    #region Serialize Field
    [Header("Paramètres du player")]
    [Tooltip("Etat du player.")]
    [SerializeField] PlayerMoveStatus _mouvementStatus = PlayerMoveStatus.NOT_MOVING;
    [Tooltip("Vitesse de marche.")]
    [SerializeField] float _walkSpeed = 1.0f;
    [Tooltip("Vitesse accroupie.")]
    [SerializeField] float _crouchSpeed = 1.0f;
    [Tooltip("Vitesse de course.")]
    [SerializeField] float _runSpeed = 4.5f;
    [Tooltip("Longueur des pas lorsque le player court.")]
    [SerializeField] float _runStepLengthen = 0.75f;
    [Tooltip("Vitesse de saut.")]
    [SerializeField] float _jumpSpeed = 4.5f;
    [Tooltip("Force appliqué pour coller le player au sol.")]
    [SerializeField] float _stickToGroundForce = 5.0f;
    [Tooltip("Gravité.")]
    [SerializeField] float _gravityMultiplier = 2.5f;
    [Tooltip("Temps minimal en seconde lors d'une chute pour que le player passe au status \"Landing\".")]
    [SerializeField] float _landingTreshold = 0.5f;
    [Tooltip("Point de depart du Raycast vérifiant si on est collé au sol ou non.")]
    [SerializeField] Transform _groundCheck = null;
    [Tooltip("Headbob")]
    [SerializeField] CurveControlledHeadBob _headBob = new CurveControlledHeadBob();
    [Tooltip("Liste des AudioSource du player.")]
    [SerializeField] List<AudioSource> _audioSources = new List<AudioSource>();
    [Tooltip("Lampe Torche.")]
    [SerializeField] GameObject _flashLight = null;
    #endregion

    #region Private
    Camera _camera = null;
    bool _jumpButtonPressed = false, _previouslyGrounded = false, _isWalking = true, _isCrouching = false, _isJumping = false;
    Vector2 _inputVector = Vector2.zero;
    Vector3 _moveDirection = Vector3.zero, _localSpaceCameraPosition = Vector3.zero;
    float _fallingTimer = 0.0f, _controllerHeight = 0.0f;
    CharacterController _characterController = null;
    int _audioToUse = 0;
    #endregion

    #region Public
    PlayerMoveStatus mouvementStatus { get { return _mouvementStatus; } }
    float walkSpeed { get { return _walkSpeed; } }
    float runSpeed { get { return _runSpeed; } }
    #endregion

    #region System
    public void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Start()
    {      
        _camera = Camera.main;
        _localSpaceCameraPosition = _camera.transform.localPosition;
        _mouvementStatus = PlayerMoveStatus.NOT_MOVING;
        _fallingTimer = 0.0f;    
        _controllerHeight = _characterController.height;

        _headBob.Initialize();  
        _headBob.RegisterEventCallBack(1.5f, PlayFootStepSound, CurveControlledBobCallBackType.VERTICAL); 

        if(_flashLight)
        {
            _flashLight.SetActive(false);
        }
    }

    public void Update()
    {
        if(_characterController.isGrounded)
        {
            _fallingTimer = 0.0f;
        }
        else
        {
            _fallingTimer += Time.deltaTime;
        }

        if(Input.GetButtonDown("FlashLight"))
        {
            _flashLight.SetActive(!_flashLight.activeSelf);
        }

        if(!_jumpButtonPressed && !_isCrouching)
        {
            _jumpButtonPressed = Input.GetButtonDown("Jump");
        }

        if(Input.GetButtonDown("Crouch"))
        {
            _isCrouching = !_isCrouching;
            _characterController.height = _isCrouching == true ? _controllerHeight/2.0f : _controllerHeight;
        }

        if(!_previouslyGrounded && _characterController.isGrounded)
        {
            if(_fallingTimer > _landingTreshold)
            {
                _moveDirection.y = 0f;
                _isJumping = false;
                _mouvementStatus = PlayerMoveStatus.LANDING;
            }       
        }
        else if(!_characterController.isGrounded)
        {
            _mouvementStatus = PlayerMoveStatus.NOT_GROUNDED;
        }
        else if(_characterController.velocity.sqrMagnitude < 0.01f)
        {
            _mouvementStatus = PlayerMoveStatus.NOT_MOVING;
        }
        else if(_isCrouching)
        {
            _mouvementStatus = PlayerMoveStatus.CROUCHING;
        }
        else if(_isWalking)
        {
            _mouvementStatus = PlayerMoveStatus.WALKING;
        }
        else
        {
            _mouvementStatus = PlayerMoveStatus.RUNNING;
        }

        _previouslyGrounded = _characterController.isGrounded;
    }

    public void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");        
        float vertical = Input.GetAxis("Vertical");
        bool wasWalking = _isWalking;
        _isWalking = !Input.GetKey(KeyCode.LeftShift);

        float speed = _isCrouching ? _crouchSpeed : (_isWalking ? _walkSpeed : _runSpeed);
        _inputVector = new Vector2(horizontal, vertical);

        if(_inputVector.sqrMagnitude > 1)
        {
            _inputVector.Normalize();
        }

        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;
        
        RaycastHit hitInfo;
       
        if (Physics.SphereCast(_groundCheck.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height, 1))
        {
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        }
        _moveDirection.x = desiredMove.x * speed;
        _moveDirection.z = desiredMove.z * speed;

        if (_characterController.isGrounded)
        {
            _moveDirection.y = -_stickToGroundForce;

            if (_jumpButtonPressed)
            {
                _moveDirection.y = _jumpSpeed;
                _jumpButtonPressed = false;
                _isJumping = true;
            }
        }
        else
        {
           _moveDirection += Physics.gravity * _gravityMultiplier * Time.deltaTime;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);

        Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z);

        if(speedXZ.magnitude > 0.01f)
        {
            _camera.transform.localPosition = _localSpaceCameraPosition + _headBob.GetVectorOffset(speedXZ.magnitude * ((_isCrouching || _isWalking) ? 1.0f : _runStepLengthen));
        }
        else
        {
             _camera.transform.localPosition = _localSpaceCameraPosition;
        }        
    }

    void PlayFootStepSound()
    {
        if(!_isCrouching)
        {
            _audioSources[_audioToUse].Play();
            _audioToUse = (_audioToUse == 0) ? 1 : 0;
        }      
    }
    #endregion
}

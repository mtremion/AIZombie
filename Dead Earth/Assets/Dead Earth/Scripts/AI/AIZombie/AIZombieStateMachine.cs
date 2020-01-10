using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateMachine : AIStateMachine
{
    #region Public
    public float fov { get { return _fov; } }
    public float sight { get { return _sight; } }
    public float hearing { get { return _hearing; } }
    public float aggresion { get { return aggresion; } set { _aggression = value; } }
    public int health { get { return _health; } set { _health = value; } }
    public float intelligence { get { return _intelligence; } }
    public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
    public float replenishRate { get { return _replenishRate; } }
    public float hungryPercent { get { return _hungryPercent; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public bool feeding { get { return _feeding; } set { _feeding = value; } }
    public bool crawling { get { return _crawling; } set { _crawling = value; } }
    public float idIdle { get { return _idIdle; } set { _idIdle = value; } }
    public float idWalk { get { return _idWalk; } set { _idWalk = value; } }
    public float idRun { get { return _idRun; } set { _idRun = value; } }
    public float idPursuit { get { return _idPursuit; } set { _idPursuit = value; } }
    public float speed { get { return _speed; } set { _speed = value; } }
    public int idAttack { get { return _idAttack; } set { _idAttack = value; } }
    #endregion

    #region Serialize Field
    [Header("Paramètre de l'entité")]
    [Tooltip("Angle de vision de l'entité.")]
    [Range(10.0f, 360.0f)]
    [SerializeField] float _fov = 60.0f;
    [Tooltip("Distance à laquelle l'entité peut voir. Plus la valeure est grande, plus l'entité voit loin.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float _sight = 0.5f;
    [Tooltip("Distance à laquelle l'entité peut entendre. Plus la valeure est grande, plus l'entité entend de loin.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float _hearing = 1.0f;
    [Tooltip("Aggressivité de l'entité. Plus la valeure est grande, plus l'entité est aggressive")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float _aggression = 0.5f;
    [Tooltip("Point de vie de l'entité.")]
    [Range(0, 300)]
    [SerializeField] int _health = 100;
    [Tooltip("Intelligence de l'entité. Plus la valeur est grande, plus l'entité a des chances de localiser un son/une cible.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float _intelligence = 0.5f;
    [Tooltip("Jauge de faim de l'entité. Décrémente progressivement, une fois à 0 l'entité doit se rassasier.")]
    [Range(0.0f,1.0f)]
    [SerializeField] float _satisfaction = 1.0f;
    [Tooltip("Pourcentage auquel se remplit la jauge de satisfaction par seconde lorsque l'entité se nourrit.")]
    [SerializeField] float _replenishRate = 0.5f;
    [Tooltip("Pourcentage auquel se vide la jauge de satisfaction par seconde dans le temps.")]
    [SerializeField] float _depletionRate = 0.1f;
    [Tooltip("Pourcentage auquel l'entité aura envie de manger.")]
    [Range(0.0f,100.0f)]
    [SerializeField] float _hungryPercent = 50.0f;
    [Tooltip("L'entité est elle un rampant ?")]
    [SerializeField] bool _isCrawler = false;
    [Tooltip("Première Target")]
    [SerializeField] GameObject _firstTarget = null;
    [Header("Paramètre des choix d'animations. (Idle/Walk/Run/Pursuit)")]
    [Tooltip("Choix de l'animation de base à l'état Idle.")]
    [Range(1, 3)]
    [SerializeField] int _idle = 1;
    [Tooltip("Choix de l'animation de base à l'état Walk.")]
    [Range(1, 3)]
    [SerializeField] int _walk = 1;
    [Tooltip("Choix de l'animation de base à l'état Run.")]
    [Range(1, 3)]
    [SerializeField] int _run = 1;
    [Tooltip("Choix de l'animation de base à l'état Pursuit.")]
    [Range(1, 3)]
    [SerializeField] int _pursuit = 1;
    #endregion

    #region Private
    int _seeking = 0;
    bool _feeding = false, _crawling = false;
    int _idAttack = 0;
    float _idIdle = 0.0f, _idWalk = 0.0f, _idRun = 0.0f, _idPursuit = 0.0f, _speed = 0.0f;
    #endregion

    #region Hashes
    int _speedHash = Animator.StringToHash("speed");
    int _seekingHash = Animator.StringToHash("seeking");
    int _feedingHash = Animator.StringToHash("isFeeding");
    int _idleHash = Animator.StringToHash("idIdle");
    int _walkHash = Animator.StringToHash("idWalk");
    int _runHash = Animator.StringToHash("idRun");
    int _pursuitHash = Animator.StringToHash("idPursuit");
    int _attackHash = Animator.StringToHash("idAttack");
    int _crawlingHash = Animator.StringToHash("isCrawling");
    #endregion

    #region System
    protected override void Start()
    {
        base.Start();

        _idIdle = _idle;
        _idWalk = _walk;
        _idRun = _run;
        _idPursuit = _pursuit;
        _crawling = _isCrawler; 

        if (_animator != null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetBool(_crawlingHash, _crawling);
            _animator.SetFloat(_speedHash, _navMeshAgent.speed);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetFloat(_idleHash, _idIdle);
            _animator.SetFloat(_walkHash, _idWalk);
            _animator.SetFloat(_runHash, _idRun);
            _animator.SetFloat(_pursuitHash, _idPursuit);
            _animator.SetInteger(_attackHash, _idAttack);
        }


        if(_firstTarget != null)
        {
            float distance = Vector3.Distance(transform.position, _firstTarget.transform.position);
            SetTarget(AITargetType.WAYPOINT, null, _firstTarget.transform.position, distance);
        }
    }
    protected override void Update()
    {
        base.Update();

        if(_animator != null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _idAttack);
            _animator.SetFloat(_idleHash, _idIdle);
            _animator.SetFloat(_walkHash, _idWalk);
            _animator.SetFloat(_runHash, _idRun);
            _animator.SetFloat(_pursuitHash, _idPursuit);
        }

        satisfaction = Mathf.Max(0, _satisfaction - (((_satisfaction - Time.deltaTime)/100.0f)*_speed)/100.0f);
    }
    #endregion

    #region Main Methods

    #endregion
}

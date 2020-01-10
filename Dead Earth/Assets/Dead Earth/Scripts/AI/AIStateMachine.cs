using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region Enum
public enum AIStateType {None, IDLE, ALERTED, PATROL, ATTACK, FEEDING, PURSUIT, DEAD}
public enum AITargetType {None, WAYPOINT, VISUAL_PLAYER, VISUAL_LIGHT, VISUAL_FOOD, AUDIO}
public enum AITriggerEventType {ENTER, STAY, EXIT}
#endregion

#region Struct
public struct AITarget
{
    #region Private
    AITargetType _type; // The type of target
    Collider _collider; // The collider
    Vector3 _position; // Current position in the world
    float _distance; // Distance from player
    float _time; // Time the target was last pinged
    #endregion

    #region Main Methods
    // Public
    public AITargetType type { get { return _type; } }
    public Collider collider { get { return _collider; } }
    public Vector3 position {  get { return _position; } }
    public float distance { get { return _distance; } set { _distance = value; } }
    public float time { get { return _time; } }
    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }
    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _distance = Mathf.Infinity;
        _time = 0.0f;
    }
    #endregion
}

#endregion

public abstract class AIStateMachine : MonoBehaviour
{
    #region Public
    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();
    #endregion

    #region Serialize Field
    [Header("AI Parameters")]
    [Tooltip("The current state of the entity.")] 
    [SerializeField] protected AIStateType _currentStateType = AIStateType.IDLE;
    [Tooltip("The sphere collider of the Target Trigger.")]
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [Tooltip("The sphere collider of the Sensor.")]
    [SerializeField] protected SphereCollider _sensorTrigger = null;
    [Tooltip("The stopping distance of the component NavMeshAgent of the entity.")]
    [Range(0.0f,15.0f)]
    [SerializeField] protected float _stoppingDistance = 1.0f;
    [Tooltip("Waypoint Network correspondant à l'entité.")]
    [SerializeField] protected WaypointNetworkScript _waypointNetwork = null;
    [Tooltip("Patrouille aléatoire ou non.")]
    [SerializeField] protected bool _randomPatrol = false;
    [Tooltip("Waypoint actuel.")]
    [SerializeField] protected int _currentWaypoint = -1;
    [Tooltip("L'entité patrouille en continue ou repasse à l'état idle lorsqu'elle arrive à un waypoint ?")]
    [SerializeField] bool _continuePatrol = false;
    #endregion

    #region Protected
    protected AIState _currentState = null;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRefCount = 0;
    protected bool _isTargetReached = false;

    //Component Cache
    protected Animator _animator = null;
    protected NavMeshAgent _navMeshAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;
    #endregion

    #region Main Methods
    // Public 
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navMeshAgent {  get { return _navMeshAgent; } }
    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }
    public bool isTargetReached { get { return _isTargetReached; } }
    public bool continuePatrol { get { return _continuePatrol; } }
    public bool inMeleeRange; //{ get; set; } 
    public AITargetType targetType { get { return _target.type; } }
    public Vector3 targetPosition { get { return _target.position; } }
    public int targetColliderID 
    { 
        get
        {
            if(_target.collider)
            {
                return _target.collider.GetInstanceID();
            }
            else
            {
                return -1;
            }
        }
    }
    public Vector3 sensorPosition
    {
        get
        {
            if (_sensorTrigger == null)
            {
                return Vector3.zero;
            }
            Vector3 point = _sensorTrigger.transform.position;
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;

            return point;
        }
    }
    public float sensorRadius
    {
        get
        {
            if (_sensorTrigger == null)
            {
                return 0.0f;
            }

            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);

            return Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z, radius);
        }
    }
    public void SetTarget(AITarget t)
    {
        _target = t;

        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        _target.Set(t, c, p, d);

        if(_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        _target.Set(t, c, p, d);

        if (_targetTrigger != null)
        {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }
    public void ClearTarget()
    {
        _target.Clear();

        if(_targetTrigger != null)
        {
            _targetTrigger.enabled = false;
        }
    }
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if(_navMeshAgent)
        {
            _navMeshAgent.updatePosition = positionUpdate;
            _navMeshAgent.updateRotation = rotationUpdate;
        }
    }
    public void AddRootMotiionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }
    // Protected
    protected virtual void Awake()
    {
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        if(GameSceneManager.instance != null)
        {
            if(_collider)
            {
                GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            }
            
            if(_sensorTrigger)
            {
                GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
            }
        }
    }
    protected virtual void Start()
    {
        if(_sensorTrigger != null)
        {
            AISensor aiSensorScript = _sensorTrigger.GetComponent<AISensor>();
            
            if(aiSensorScript != null)
            {
                aiSensorScript.parentStateMachine = this;
            }
        }

        AIState[] states = GetComponents<AIState>();

        foreach(AIState state in states)
        {
            if(state != null && !_states.ContainsKey(state.GetStateType()))
            {
                _states[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
        }

        if(_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }

        if(_animator)
        {
            AIStateMachineLink[] aiStateMachineLinkScripts= _animator.GetBehaviours<AIStateMachineLink>();
            
            foreach(AIStateMachineLink script in aiStateMachineLinkScripts)
            {
                script.stateMachine = this;
            }
        }
    }
    protected virtual void FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if(_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }

        _isTargetReached = false;
    }
    protected virtual void Update()
    {
        if (_currentState == null)
        {
            return;
        }

        AIStateType newStateType = _currentState.OnUpdate();
        
        if(newStateType != _currentStateType)
        {
            AIState newState = null;

            if(_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else if(_states.TryGetValue(AIStateType.IDLE, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if(_targetTrigger == null || other != _targetTrigger)
        {
            return;
        }

        _isTargetReached = true;

        if(_currentState != null)
        {
            _currentState.OnDestinationReached(true);
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger)
        {
            return;
        }

        _isTargetReached = false;

        if (_currentState != null)
        {
            _currentState.OnDestinationReached(false);
        }
    }
    protected void OnTriggerStay(Collider other)
    {
        if(_targetTrigger == null || other != _targetTrigger)
        {
            return;
        }

        _isTargetReached = true;
    }
    public virtual void OnTriggerEventType(AITriggerEventType type, Collider other)
    { 
        if(_currentState != null)
        {
            _currentState.OnTriggerEvent(type, other);
        }
    }
    protected virtual void OnAnimatorMove()
    {
        if(_currentState != null)
        {
            _currentState.OnAnimatorUpdated();
        }
    }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null)
        {
            _currentState.OnAnimatorIKUpdated();
        }
    }
    public Vector3 GetWaypointPosition(bool increment)
    {
        if(_currentWaypoint == -1)
        {
            if(_randomPatrol)
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.GetWaypointsSize());
            }
            else
            {
                _currentWaypoint = 0;
            }

            if (_waypointNetwork.GetTransformAtIndex(_currentWaypoint) != null)
            {
                Transform newWaypoint = _waypointNetwork.GetTransformAtIndex(_currentWaypoint);
                float distance = Vector3.Distance(transform.position, newWaypoint.position);
                SetTarget(AITargetType.WAYPOINT, null, newWaypoint.position, distance);

                return newWaypoint.position;
            }
        }
        else if(increment)
        {
            NextWaypoint();
        }

        if (_waypointNetwork.GetTransformAtIndex(_currentWaypoint) != null)
        {
            Transform newWaypoint = _waypointNetwork.GetTransformAtIndex(_currentWaypoint);
            float distance = Vector3.Distance(transform.position, newWaypoint.position);
            SetTarget(AITargetType.WAYPOINT, null, newWaypoint.position, distance);

            return newWaypoint.position;
        }

        return Vector3.zero;
    }
    public void NextWaypoint()
    {
        if (_randomPatrol && _waypointNetwork.GetWaypointsSize() > 1)
        {
            int oldWaypoint = _currentWaypoint;

            while (_currentWaypoint == oldWaypoint)
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.GetWaypointsSize());
            }
        }
        else
        {
            _currentWaypoint = _currentWaypoint == _waypointNetwork.GetWaypointsSize() - 1 ? 0 : _currentWaypoint + 1;
        }
    }
    #endregion
}

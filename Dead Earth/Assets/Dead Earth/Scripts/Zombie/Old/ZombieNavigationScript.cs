using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieNavigationScript : MonoBehaviour
{
    #region Serialize Field
    [Header("Caractérisque de la navigation")]
    [Tooltip("Le network correspondant à l'objet")]
    [SerializeField]
    WaypointNetworkScript waypointNetworkScript = null;
    [SerializeField]
    AnimationCurve animationCurve = new AnimationCurve();
    [SerializeField]
    float durationJump = 1.0f;
    [SerializeField]
    bool mixedMode = true;

    #endregion

    #region Private & protected
    NavMeshAgent _navAgent = null;
    NavMeshPathStatus _pathStatus;
    Coroutine _CoroutineJump = null;
    Animator _animator;
    bool _hasPath, _pathPending, _isPathStale, _isCoroutineJumpActive;
    int _currentIndex;
    float _pathRemainingDistance, _pathStoppingDistance, _timerJump, _smoothAngle, _speed;
    #endregion

    #region System
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _navAgent.updateRotation = false;

        if (waypointNetworkScript.GetTransformAtIndex(_currentIndex) != null)
        {
            _navAgent.destination = waypointNetworkScript.GetTransformAtIndex(_currentIndex).position;
        }

        SetDestination(false);

        _timerJump = 0.0f;
        _isCoroutineJumpActive = false;
    }
    // Update is called once per frame
    void Update()
    {
        _hasPath = _navAgent.hasPath;
        _pathPending = _navAgent.pathPending;
        _isPathStale = _navAgent.isPathStale;
        _pathStatus = _navAgent.pathStatus;
        _pathRemainingDistance = _navAgent.remainingDistance;
        _pathStoppingDistance = _navAgent.stoppingDistance;

        Vector3 localDesiredVelocity = transform.InverseTransformVector(_navAgent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;
        _smoothAngle = Mathf.MoveTowardsAngle(_smoothAngle, angle, 80.0f * Time.deltaTime);

        _speed = localDesiredVelocity.z;

        _animator.SetFloat("angle", _smoothAngle);
        _animator.SetFloat("speed", _speed, 0.1f, Time.deltaTime);

        if(_navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            if (!mixedMode || (mixedMode && Mathf.Abs(angle) < 80.0f && _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.locomotion")))
            {
                Quaternion lookRotation = Quaternion.LookRotation(_navAgent.desiredVelocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 2.5f * Time.deltaTime);
            }            
        }
       

        if (_navAgent.isOnOffMeshLink && _isCoroutineJumpActive == false)
        {
            _CoroutineJump = StartCoroutine(Jump(durationJump));
        }

        if ((_pathRemainingDistance <= _pathStoppingDistance && !_pathPending) || _pathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetDestination(true);
        }
        else if(_isPathStale)
        {
            SetDestination(false);
        }

        Debug.Log(_currentIndex);
    }

    private void OnAnimatorMove()
    {
        if(mixedMode && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.locomotion"))
        {
            transform.rotation = _animator.rootRotation;
        }

        _navAgent.velocity = _animator.deltaPosition / Time.deltaTime;
    }
    #endregion

    #region Main Methods
    
    void SetDestination(bool increment)
    {
        if(!waypointNetworkScript)
        {
            return;
        }

        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        while(nextWaypointTransform == null)
        {
            int nextWaypoint = (_currentIndex + incStep >= waypointNetworkScript.GetWaypointsSize()?0: _currentIndex + incStep);
            nextWaypointTransform = waypointNetworkScript.GetTransformAtIndex(nextWaypoint);

            if(nextWaypointTransform != null)
            {
                _currentIndex = nextWaypoint;
                _navAgent.destination = nextWaypointTransform.position;
                return;
            }
        }

        _currentIndex++;
    }

    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);

        _isCoroutineJumpActive = true;

        while (_timerJump <= duration)
        {
            float t = _timerJump / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (animationCurve.Evaluate(t) * Vector3.up);
            _timerJump += Time.deltaTime;
            yield return null;
        }

        _navAgent.transform.position = endPos;        
        _navAgent.CompleteOffMeshLink();
        _isCoroutineJumpActive = false;
        _timerJump = 0.0f;
        yield break;
    }
    #endregion
}

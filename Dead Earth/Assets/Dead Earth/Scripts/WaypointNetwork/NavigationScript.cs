using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavigationScript : MonoBehaviour
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

    #endregion

    #region Private & protected
    NavMeshAgent _navAgent = null;
    NavMeshPathStatus _pathStatus;
    Coroutine _CoroutineJump = null;
    bool _hasPath, _pathPending, _isPathStale, _isCoroutineJumpActive;
    int _currentIndex;
    float _pathRemainingDistance, _pathStoppingDistance, _timerJump;
    #endregion

    #region System
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(waypointNetworkScript == null)
        {
            return;
        }

        if(waypointNetworkScript.GetTransformAtIndex(_currentIndex) != null)
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

        if(_navAgent.isOnOffMeshLink && _isCoroutineJumpActive == false)
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

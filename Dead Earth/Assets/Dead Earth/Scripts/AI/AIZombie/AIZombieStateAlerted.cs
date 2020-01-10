using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateAlerted : AIZombieState
{
    #region Serialize Field
    [Header("Paramètres de l'état Alerted")]
    [Tooltip("Temps maximal en seconde durant lequel l'entité va rester dans l'état Alerted.")]
    [Range(1,60)]
    [SerializeField] float _maxDuration = 30.0f;
    [Tooltip("Temps en seconde entre 2 mouvement de rotation left/right.")]
    [SerializeField] float _timeBetweenRotation = 1.5f;
    [Tooltip("Angle dans lequel doit se trouver le zombie pour trouver son prochain waypoint.")]
    [SerializeField] float _waypointAngleThreshold = 25.0f;
    [Tooltip("Angle dans lequel doit se trouver le zombie pour trouver sa menace.")]
    [SerializeField] float _threatAngleThreshold = 10.0f;

    #endregion

    #region Private
    float _timer = 0.0f;
    float _timerRotation = 0.0f;
    #endregion

    #region Main Methods
    public override AIStateType GetStateType()
    {
        return AIStateType.ALERTED;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_zombieStateMachine == null)
        {
            return;
        }

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0.0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.idAttack = 0;

        _timer = _maxDuration;
        _timerRotation = 0.0f;
    }

    public override AIStateType OnUpdate()
    {
        _timer -= Time.deltaTime;
        _timerRotation += Time.deltaTime;

        if(_timer <= 0)
        {
            _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navMeshAgent.isStopped = false;
            _timer = _maxDuration;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_PLAYER)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.PURSUIT;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.AUDIO)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            _timer = _maxDuration;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_LIGHT)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            _timer = _maxDuration;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.None && _zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_FOOD
            && _zombieStateMachine.targetType == AITargetType.None)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.PURSUIT;
        }

        float angle;

        if((_zombieStateMachine.targetType == AITargetType.AUDIO || _zombieStateMachine.targetType == AITargetType.VISUAL_LIGHT) && !_zombieStateMachine.isTargetReached)
        {
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);

            if(_zombieStateMachine.targetType == AITargetType.AUDIO && Mathf.Abs(angle) < _threatAngleThreshold)
            {
                return AIStateType.PURSUIT;
            }

            if (_timerRotation >= _timeBetweenRotation)
            {
                if (Random.value < _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }

                _timerRotation = 0.0f;
            }
        }
        else if(_zombieStateMachine.targetType == AITargetType.WAYPOINT && !_zombieStateMachine.navMeshAgent.pathPending)
        {
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward, /*_zombieStateMachine.navMeshAgent.steeringTarget*/ _zombieStateMachine.targetPosition);

            if(Mathf.Abs(angle) < _waypointAngleThreshold)
            {              
                return AIStateType.PATROL;
            }

            if(_timerRotation > _timeBetweenRotation)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                _timerRotation = 0.0f;
            }          
        }
        else
        {
            if(_timerRotation > _timeBetweenRotation)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                _timerRotation = 0.0f;
            }
        }

        return AIStateType.ALERTED;
    }
    #endregion
}

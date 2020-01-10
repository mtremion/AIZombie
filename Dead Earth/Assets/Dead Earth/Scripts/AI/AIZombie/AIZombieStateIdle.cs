using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateIdle : AIZombieState
{
    #region Serialize Field
    [Header("Paramètre de l'état Idle.")]
    [Tooltip("Temps minimal et maximal dans lequel le zombie sera dans cet état. La valeur sera choisis aléatoirement entre min et max.")]
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);
    #endregion

    #region Private
    float _idleTime = 0.0f, _timer = 0.0f;
    #endregion

    #region Protected

    #endregion

    #region System

    #endregion

    #region Main Methods
    public override AIStateType GetStateType()
    {
        return AIStateType.IDLE;
    }
    public override void OnEnterState()
    {
        base.OnEnterState();

        if(_zombieStateMachine == null)
        {
            return;         
        }

        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0.0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.idAttack = 0;

        if(_zombieStateMachine.targetType != AITargetType.WAYPOINT)
        {
            _zombieStateMachine.ClearTarget();
        }      
    }
    public override AIStateType OnUpdate()
    {
        if(_zombieStateMachine == null)
        {
            return AIStateType.IDLE;
        }

        if(_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_PLAYER)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.PURSUIT;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_LIGHT)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.ALERTED;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.AUDIO)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.ALERTED;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_FOOD)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.PURSUIT;
        }

        _timer += Time.deltaTime;

        if(_timer >= _idleTime)
        {
            _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navMeshAgent.isStopped = false;
            return AIStateType.ALERTED;
        }

        return AIStateType.IDLE;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieStatePatrol : AIZombieState
{
    #region Public
    #endregion

    #region Serizalize Field
    [Header("Paramètre de l\'état Patrol.")]
    [Tooltip("Angle à partir duquel l'entité passe en mode alerte lorsqu'il veut changer de direction.")]
    [SerializeField] float _turnAngleThreshold = 80.0f;
    [Tooltip("Vitesse lors de la patrouille.")]
    [Range(0,3)]
    [SerializeField] float _speed = 1.0f;
    [Tooltip("Vitesse à laquelle tourne l'entité sur elle même.")]
    [SerializeField] float _slerpSpeed = 3.0f;
    #endregion

    #region Private
    bool _destinationReached;
    #endregion

    #region Main Methods
    public override AIStateType GetStateType()
    {
        return AIStateType.PATROL;
    }
    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_zombieStateMachine == null)
        {
            return;
        }

        _destinationReached = false;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.idAttack = 0;

        _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
        
        _zombieStateMachine.navMeshAgent.isStopped = false;
    }
    public override AIStateType OnUpdate()
    {
        if (_zombieStateMachine == null)
        {
            return AIStateType.PATROL;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_PLAYER)
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
            if((1.0f - _zombieStateMachine.satisfaction ) > (_zombieStateMachine.VisualThreat.distance/_zombieStateMachine.sensorRadius))
            {
                _zombieStateMachine.SetTarget(_stateMachine.VisualThreat);
                return AIStateType.PURSUIT;
            }
        }

        if(_zombieStateMachine.navMeshAgent.pathPending)
        {
            _zombieStateMachine.speed = 0.0f;
            return AIStateType.PATROL;
        }
        else
        {
            _zombieStateMachine.speed = _speed;
        }

        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.navMeshAgent.steeringTarget - _zombieStateMachine.transform.position));

        if(!_zombieStateMachine.useRootRotation)
        {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navMeshAgent.desiredVelocity);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime* _slerpSpeed);
        }

        if((_zombieStateMachine.navMeshAgent.isPathStale || (!_zombieStateMachine.navMeshAgent.hasPath && !_zombieStateMachine.navMeshAgent.pathPending) || _zombieStateMachine.navMeshAgent.pathStatus != NavMeshPathStatus.PathComplete))
        {
            _zombieStateMachine.GetWaypointPosition(true);          
        }

       if (_destinationReached)
       {
           if(!_zombieStateMachine.continuePatrol)
           {
                return AIStateType.IDLE;
           }
                   
           if(_zombieStateMachine.targetType == AITargetType.WAYPOINT)
           {
               _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(true));
           }
       }

        return AIStateType.PATROL;
    }
    public override void OnExitState()
    {
        if(!_zombieStateMachine.continuePatrol && _zombieStateMachine.targetType == AITargetType.WAYPOINT)
        {
            _zombieStateMachine.NextWaypoint();
        }       
    }
    public override void OnDestinationReached(bool isReached)
    {
        _destinationReached = isReached;

        if (_zombieStateMachine == null || !isReached)
        {
            return;
        }  
    }
    #endregion
}

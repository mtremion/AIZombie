using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieStatePursuit : AIZombieState
{
    #region Serialize Field
    [Header("Paramètres de l'état Pursuit")]
    [Tooltip("Vitesse à laquelle l'entité va poursuivre la cible.")]
    [Range(1, 10)]
    [SerializeField] float _speed = 1.0f;
    [Tooltip("Variable adoucissant la rotation lors de la poursuite.")]
    [SerializeField] float _slerpSpeed = 5.0f;
    [Tooltip("Temps entre deux recalcul de chemin entre l'entité et la cible. Plus on est proche de la cible, plus le path sera recalculé.")]
    [SerializeField] float _repathDistanceMultiplier = 0.35f;
    [Tooltip("Temps minimal de la vérification si l'entité \"voit\" encore la cible.")]
    [SerializeField] float _repathVisualMinDuration = 0.05f;
    [Tooltip("Temps maximal de la vérification si l'entité \"voit\" encore la cible.")]
    [SerializeField] float _repathVisualMaxDuration = 5.0f;
    [Tooltip("Temps minimal de la vérification si l'entité \"entend\" encore la cible.")]
    [SerializeField] float _repathAudioMinDuration = 0.05f;
    [Tooltip("Temps maximal de la vérification si l'entité \"entend\" encore la cible.")]
    [SerializeField] float _repathAudioMaxDuration = 5.0f;
    [Tooltip("Temps maximal dans lequel l'entité restera à l'état Pursuit.")]
    [SerializeField] float _maxTimePursuit = 60.0f;
    #endregion

    #region Private
    float _timer = 0.0f,_repathTimer = 0.0f;
    #endregion

    #region Protected

    #endregion

    #region System
    public override AIStateType GetStateType()
    {
        return AIStateType.PURSUIT;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if(_zombieStateMachine == null)
        {
            return;
        }

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.idAttack = 0;

        _timer = 0.0f;
        _repathTimer = 0.0f;

        _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navMeshAgent.isStopped = false;
    }

    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer >= _maxTimePursuit)
        {
            return AIStateType.PATROL;
        }

        if(_stateMachine.targetType == AITargetType.VISUAL_PLAYER && _zombieStateMachine.inMeleeRange)
        {
            return AIStateType.ATTACK;
        }

        if(_zombieStateMachine.isTargetReached)
        {
            switch(_stateMachine.targetType)
            {
                case AITargetType.AUDIO:
                    _zombieStateMachine.ClearTarget();
                    return AIStateType.ALERTED;
                case AITargetType.VISUAL_LIGHT:
                    _zombieStateMachine.ClearTarget();
                    return AIStateType.ALERTED;
                case AITargetType.VISUAL_FOOD:
                    return AIStateType.FEEDING;
            }        
        }

        if (_zombieStateMachine.navMeshAgent.isPathStale || (!_zombieStateMachine.navMeshAgent.hasPath && !_zombieStateMachine.navMeshAgent.pathPending) || _zombieStateMachine.navMeshAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.ALERTED;
        }

        if(_zombieStateMachine.navMeshAgent.pathPending)
        {
            _zombieStateMachine.speed = 0.0f;
        }
        else
        {
            _zombieStateMachine.speed = _speed;

            if (!_zombieStateMachine.useRootRotation && _zombieStateMachine.targetType == AITargetType.VISUAL_PLAYER
            && _zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_PLAYER && _zombieStateMachine.isTargetReached)
            {
                Vector3 targetPos = _zombieStateMachine.targetPosition;
                targetPos.y = _zombieStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.targetPosition);
                _zombieStateMachine.transform.rotation = newRot;
            }
            else if (!_zombieStateMachine.useRootRotation && !_zombieStateMachine.isTargetReached)
            {
                Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navMeshAgent.desiredVelocity);
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, _slerpSpeed);
            }
            else if (_zombieStateMachine.isTargetReached)
            {
                return AIStateType.ALERTED;
            }
        }

        if(_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_PLAYER)
        {
            if(_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
            {
                if(Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)                
                {
                    _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                    _repathTimer = 0.0f;
                }
            }

            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);

            return AIStateType.PURSUIT;
        }

        if(_zombieStateMachine.targetType == AITargetType.VISUAL_PLAYER)
        {
            return AIStateType.PURSUIT;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_LIGHT)
        {
            if (_zombieStateMachine.targetType == AITargetType.AUDIO || _zombieStateMachine.targetType == AITargetType.VISUAL_FOOD)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.ALERTED;
            }
            else if (_zombieStateMachine.targetType == AITargetType.VISUAL_LIGHT)
            {
                int currentID = _zombieStateMachine.targetColliderID;

                if (currentID == _zombieStateMachine.VisualThreat.collider.GetInstanceID())
                {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
                    {
                        if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                        {
                            _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);

                    return AIStateType.PURSUIT;
                }
                else
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                    return AIStateType.ALERTED;
                }
            }
        }
        else if (_zombieStateMachine.AudioThreat.type == AITargetType.AUDIO)
        {
            if (_zombieStateMachine.targetType == AITargetType.VISUAL_FOOD)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                return AIStateType.ALERTED;
            }
            else if (_zombieStateMachine.targetType == AITargetType.AUDIO)
            {
                int currentID = _zombieStateMachine.targetColliderID;

                if (currentID == _zombieStateMachine.AudioThreat.collider.GetInstanceID())
                {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.AudioThreat.position)
                    {
                        if (Mathf.Clamp(_zombieStateMachine.AudioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
                        {
                            _zombieStateMachine.navMeshAgent.SetDestination(_zombieStateMachine.AudioThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                    return AIStateType.PURSUIT;
                }
                else
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                    return AIStateType.ALERTED;
                }
            }
        }

        return AIStateType.PURSUIT;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateAttack : AIZombieState
{
     #region Serialize Field
    [Header("Paramètres de l'état Attack")]
    [Tooltip("Vitesse à laquelle l'entité va se déplacer quand il attaquera.")]
    [SerializeField] float _speed = 0.0f;
    [Tooltip("Valeur adoussicant la vitesse de rotation de l'entité.")]
    [SerializeField] float _slerpSpeed = 3.0f;
    #endregion

    #region Private
    #endregion

    #region Main Methods
    public override AIStateType GetStateType()
    {
        return AIStateType.ATTACK;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_zombieStateMachine == null)
        {
            return;
        }

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.idAttack = Random.Range(1,100);
    }

    public override AIStateType OnUpdate()
    {        
        Vector3 targetPos = new Vector3();
        Quaternion newRot;      

        if(_zombieStateMachine.VisualThreat.type == AITargetType.VISUAL_PLAYER)
        {
            _zombieStateMachine.SetTarget(_stateMachine.VisualThreat);

            Debug.Log(_zombieStateMachine.inMeleeRange);

            if(!_zombieStateMachine.inMeleeRange)
            {
                return AIStateType.PURSUIT;
            }

            if(!_zombieStateMachine.useRootRotation)
            {
                targetPos = _zombieStateMachine.targetPosition;
                targetPos.y = _zombieStateMachine.transform.position.y;
                newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            }

            _zombieStateMachine.idAttack = Random.Range(1,100);

            return AIStateType.ATTACK;
        }

        if(!_zombieStateMachine.useRootRotation)
        {
            targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        return AIStateType.ALERTED;
    }

    public override void OnExitState()
    {
        _zombieStateMachine.idAttack = 0;
    }
    #endregion
}

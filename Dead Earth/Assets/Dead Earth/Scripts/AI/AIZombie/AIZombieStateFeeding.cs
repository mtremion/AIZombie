using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateFeeding : AIZombieState
{
    #region Serialize Field
    [Header("Paramètre de l'état Feeding")]
    [Tooltip("Valeur ajustant la vitesse de rotation.")]
    [SerializeField] float _slerpSpeed = 5.0f;
    [Tooltip("Point d'emission des particules de sang.")]
    [SerializeField] GameObject _bloodParticulesMount = null;
    #endregion

    #region Private
    int _eatingStateHash = Animator.StringToHash("Feeding State"), _eatingLayerIndex = -1;
    float _timer = 0.0f;
    #endregion

    #region System
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_zombieStateMachine == null)
        {
            return;
        }

        Debug.Log("Enter in Feeding State");

        if (_eatingLayerIndex == -1)
        {
            _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("Cinematic");
        }

        _zombieStateMachine.feeding = true;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.idAttack = 0;

        _timer = 0.0f;

        _zombieStateMachine.NavAgentControl(true, false);
    }
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;

        if(_zombieStateMachine.satisfaction > 0.9f)
        {
            _zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.ALERTED;
        }

        if(_zombieStateMachine.VisualThreat.type != AITargetType.None && _zombieStateMachine.VisualThreat.type != AITargetType.VISUAL_FOOD)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.ALERTED;
        }

        if(_zombieStateMachine.AudioThreat.type == AITargetType.AUDIO)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.ALERTED;
        }

        if(_zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash == _eatingStateHash)
        {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + (Time.deltaTime * _zombieStateMachine.replenishRate)/100.0f,1.0f);
            _bloodParticulesMount.SetActive(true);
        }

        if(!_zombieStateMachine.useRootRotation)
        {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        return AIStateType.FEEDING;
    }
    public override void OnExitState()
    {
        if(_zombieStateMachine == null)
        {
            return;
        }

        _zombieStateMachine.feeding = false;
        _bloodParticulesMount.SetActive(false);
    }
    #endregion

    #region Main Methods
    public override AIStateType GetStateType()
    {
        return AIStateType.FEEDING;
    }
    #endregion
}

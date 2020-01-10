using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConfigurator : AIStateMachineLink
{
    #region Serialize Field
    [Header("Paramètre du Root Motion")]
    [Tooltip("Position.")]
    [SerializeField] int _rootPosition = 0;
    [Tooltip("Rotation.")]
    [SerializeField] int _rootRotation = 0;
    #endregion

    #region System
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       if(_stateMachine)
        {
            _stateMachine.AddRootMotiionRequest(_rootPosition, _rootRotation);
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine)
        {
            _stateMachine.AddRootMotiionRequest(-_rootPosition, -_rootRotation);
        }
    }
    #endregion
}

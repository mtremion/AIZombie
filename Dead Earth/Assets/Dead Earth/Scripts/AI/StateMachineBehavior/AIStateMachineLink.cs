using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachineLink : StateMachineBehaviour
{
    #region Protected
    protected AIStateMachine _stateMachine;
    #endregion

    #region Main Methods
    public AIStateMachine stateMachine { set { _stateMachine = value; } }
    #endregion
}

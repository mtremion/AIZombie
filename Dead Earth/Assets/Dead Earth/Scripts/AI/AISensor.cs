using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour
{
    #region Private
    AIStateMachine _parentStateMachine = null;
    #endregion

    #region System
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if(_parentStateMachine != null)
        {
            _parentStateMachine.OnTriggerEventType(AITriggerEventType.ENTER, other);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (_parentStateMachine != null)
        {
            _parentStateMachine.OnTriggerEventType(AITriggerEventType.STAY, other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (_parentStateMachine != null)
        {
            _parentStateMachine.OnTriggerEventType(AITriggerEventType.EXIT, other);
        }
    }
    #endregion

    #region Main Methods
    public AIStateMachine parentStateMachine { set { _parentStateMachine = value; } }
    #endregion
}

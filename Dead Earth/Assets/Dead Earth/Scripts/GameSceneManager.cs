using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    #region Serialiaze Field
    [Header("Paramètres Globaux")]
    #endregion

    #region Static
    static GameSceneManager _instance = null;
    #endregion

    #region Private
    Dictionary<int, AIStateMachine> _statesMachines = new Dictionary<int, AIStateMachine>();
    #endregion

    #region System

    #endregion

    #region Main Methods
    // Public
    public static GameSceneManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));               
            }
            return _instance;
        }
    }
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if(!_statesMachines.ContainsKey(key))
        {
            _statesMachines[key] = stateMachine;
        }
    }
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine stateMachine = null;

        if(_statesMachines.TryGetValue(key, out stateMachine))
        {
            return stateMachine;
        }

        return null;
    }
    #endregion
}

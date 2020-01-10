using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlidingDoorScript : MonoBehaviour
{
    #region Public
    public enum DoorState { OPEN, ANIMATING, CLOSED };
    #endregion

    #region Serialize Field
    [Header("Caractérisque de la porte")]
    [Tooltip("Graphics de la porte.")]
    [SerializeField]
    GameObject graphicsDoor = null;
    [Tooltip("Transform où se trouve la porte quand elle est ouverte.")]
    [SerializeField]
    Transform openPoint = null;
    [Tooltip("Transform où se trouve la porte quand elle est fermée.")]
    [SerializeField]
    Transform closePoint = null;
    [Tooltip("Courbe du slide de la porte.")]
    [SerializeField]
    AnimationCurve animationCurveSlide = new AnimationCurve();
    [Tooltip("Courbe du slide de la porte.")]
    [SerializeField]
    float timerAnimationDoor = 2.0f;
    [Tooltip("Distance entre l'agent et l'obstacle avant qu'il ne s'arrête.")]
    [SerializeField]
    float moveThreshold = 0.1f;
    [Tooltip("Temps en seconde avant que l'agent ne recalcule un chemin.")]
    [SerializeField]
    float timeToStationary = 0.5f;

    #endregion

    #region Private & protected
    Transform _transform;
    Vector3 _openPos, _closedPos;
    NavMeshObstacle _navMeshObstacle;
    DoorState _currentState;
    #endregion

    #region System
    private void Awake()
    {
        _navMeshObstacle = GetComponentInChildren<NavMeshObstacle>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _transform = graphicsDoor.transform;
        _openPos = openPoint.position;
        _closedPos = closePoint.position;
        _currentState = DoorState.CLOSED;
        _navMeshObstacle.carvingMoveThreshold = moveThreshold;
        _navMeshObstacle.carvingTimeToStationary = timeToStationary;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _currentState!= DoorState.ANIMATING)
        {
            StartCoroutine(AnimateDoor((_currentState == DoorState.OPEN) ? DoorState.CLOSED : DoorState.OPEN));
        }
    }
    #endregion

    #region Main Methods
    IEnumerator AnimateDoor(DoorState newState)
    {
        _currentState = DoorState.ANIMATING;
        
        Vector3 startPos = (newState == DoorState.OPEN) ? _closedPos : _openPos;
        Vector3 endPos = (newState == DoorState.OPEN) ? _openPos : _closedPos;

        float timer = 0.0f;

        while (timer <= timerAnimationDoor)
        {          
            float t = timer / timerAnimationDoor;
            _transform.position = Vector3.Lerp(startPos, endPos, animationCurveSlide.Evaluate(t));
            
            timer += Time.deltaTime;
            yield return null;
        }

        _transform.position = endPos;      
        _currentState = newState;
    }
    #endregion
}

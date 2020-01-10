using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState
{
    #region Protected
    protected int _playerLayerMask = -1;
    protected int _bodyPartLayer = -1;
    protected int _visualLayerMask = -1;
    protected AIZombieStateMachine _zombieStateMachine = null;
    #endregion

    #region System
    private void Awake()
    {
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part")+1;
        _visualLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator") +1;
        _bodyPartLayer = LayerMask.NameToLayer("AI Body Part");       
    }
    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        if (_zombieStateMachine == null)
        {
            return;
        }       

        if(eventType != AITriggerEventType.EXIT)
        {
            AITargetType curType = _zombieStateMachine.VisualThreat.type;
            Debug.Log(curType);
            switch(other.tag)
            {
                case "Player":
                    

                    float distance = Vector3.Distance(_zombieStateMachine.sensorPosition, other.transform.position);               

                    if (curType != AITargetType.VISUAL_PLAYER || (curType == AITargetType.VISUAL_PLAYER && distance < _zombieStateMachine.VisualThreat.distance))
                    {
                        RaycastHit hitInfo;
                   
                        if (ColliderIsVisible(other, out hitInfo, _playerLayerMask))
                        {                           
                            _zombieStateMachine.VisualThreat.Set(AITargetType.VISUAL_PLAYER, other, other.transform.position, distance);
                        }
                    }

                    break;
                case "Flash Light":
                    if(curType != AITargetType.VISUAL_PLAYER)
                    {
                        Debug.Log("Une lumière est entrée dans mon sensor.");
                        BoxCollider flashLightTrigger = (BoxCollider)other;
                        float distanceToThreat = Vector3.Distance(_zombieStateMachine.sensorPosition,flashLightTrigger.transform.position);
                        float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                        float aggravationFactor = distanceToThreat / zSize;
                        RaycastHit hitInfo;

                        
                        if(aggravationFactor <= _zombieStateMachine.sight && aggravationFactor <= _zombieStateMachine.intelligence)
                        {
                            Debug.Log("Je vois la lumière !");
                            _zombieStateMachine.VisualThreat.Set(AITargetType.VISUAL_LIGHT, other, other.transform.position, distanceToThreat);
                        }
                        
                    }

                    break;

                case "AI Sound Emitter":
                    if(curType != AITargetType.VISUAL_PLAYER)
                    {
                        SphereCollider soundTrigger = (SphereCollider)other;

                        if(soundTrigger == null)
                        {
                            return;
                        }

                        Vector3 agentSensorPosition = _zombieStateMachine.sensorPosition;
                        Vector3 soundPos;
                        float soundRadius;
                        AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);
                        float distanceToThreat = (soundPos - agentSensorPosition).magnitude;
                        float distanceFactor = (distanceToThreat / soundRadius);
                        distanceFactor += distanceFactor * (1.0f - _zombieStateMachine.hearing);

                        if(distanceFactor > 1.0f)
                        {
                            return;
                        }

                        if(distanceToThreat < _zombieStateMachine.AudioThreat.distance)
                        {
                            _zombieStateMachine.AudioThreat.Set(AITargetType.AUDIO, other, soundPos, distanceToThreat);
                        }
                    }

                    break;
                case "AI Food":
                    if(curType != AITargetType.VISUAL_PLAYER && curType != AITargetType.VISUAL_LIGHT 
                        && _zombieStateMachine.AudioThreat.type == AITargetType.None && _zombieStateMachine.satisfaction <= (_zombieStateMachine.hungryPercent/100.0f))
                    {
                        float distanceToThreat = Vector3.Distance(other.transform.position, _zombieStateMachine.sensorPosition);

                        if (distanceToThreat < _zombieStateMachine.VisualThreat.distance)
                        {
                            RaycastHit hitInfo;

                            if(ColliderIsVisible(other, out hitInfo, _visualLayerMask))
                            {
                                _zombieStateMachine.VisualThreat.Set(AITargetType.VISUAL_FOOD, other, other.transform.position, distanceToThreat);
                            }

                        }
                    }
                    
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Main Methods
    public override void SetStateMachine(AIStateMachine stateMachine)
    {
        if(stateMachine.GetType() == typeof(AIZombieStateMachine))
        {
            base.SetStateMachine(stateMachine);
            _zombieStateMachine = (AIZombieStateMachine)stateMachine;
        }     
    }

    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask)
    {
        hitInfo = new RaycastHit();

        if(_zombieStateMachine == null)
        {
            return false;
        }

        Vector3 head = _zombieStateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(direction, transform.forward);

        if (angle > _zombieStateMachine.fov * 0.5f)
         {
             return false;
         }

         RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized,_zombieStateMachine.sensorRadius * _zombieStateMachine.sight, layerMask, QueryTriggerInteraction.Collide);
        Debug.DrawRay(head, direction, Color.red);
        float closestColliderDistance = float.MaxValue;
         Collider closestCollider = null;

         for (int i = 0; i< hits.Length; i++)
         {
             RaycastHit hit = hits[i];

             if (hit.distance < closestColliderDistance)
             {              
                 if (hit.transform.gameObject.layer == _bodyPartLayer)
                 {
                     if(_stateMachine != GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                     {
                         closestColliderDistance = hit.distance;
                         closestCollider = hit.collider;
                         hitInfo = hit;
                     }
                 }
                 else
                 {
                     closestColliderDistance = hit.distance;
                     closestCollider = hit.collider;
                     hitInfo = hit;
                 }
             }
         }

         if(closestCollider && closestCollider.gameObject == other.gameObject)
         {
             return true;
         }

        return false;
    }
    #endregion
}
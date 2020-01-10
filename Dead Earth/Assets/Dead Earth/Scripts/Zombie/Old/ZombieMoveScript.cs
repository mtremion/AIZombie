using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMoveScript : MonoBehaviour
{

    #region Serialize Field
    [Header("Caractéristique du Zombie :")]
    [Tooltip("Vitesse de déplacement du zombie. (20f par défault)")]
    [SerializeField] float speed = 20f;
    [Tooltip("Vitesse de rotation du zombie. (50f par défault)")]
    [SerializeField] float rotationSpeed = 50f;

    #endregion

    #region Private
    Animator _animator;
    float _horizontal, _vertical, _actualSpeed, _actualRotationSpeed;
    #endregion

    #region System
    // Start is called before the first frame update
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    #endregion

    #region Main Methods
    void Move()
    {
        if(Input.GetKey(KeyCode.LeftControl))
        {
            _actualSpeed = 100f;
            _actualRotationSpeed = 100f;
        }
        else
        {
            _actualSpeed = speed;
            _actualRotationSpeed = rotationSpeed;
        }

        _horizontal = Input.GetAxis("Horizontal") * 2.32f * _actualRotationSpeed * Time.deltaTime;
        _vertical = Input.GetAxis("Vertical") * 5.66f * _actualSpeed * Time.deltaTime;

        _animator.SetFloat("horizontal", _horizontal);
        _animator.SetFloat("vertical", _vertical);
    }
    #endregion
}

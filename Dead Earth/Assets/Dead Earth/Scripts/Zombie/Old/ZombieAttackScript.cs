using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackScript : MonoBehaviour 
{

    #region Private
    Animator _animator;
    bool _isAttacking = false;
    #endregion

    #region System
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Attack();
    }
    #endregion

    #region Main 
    //public
    public void EndAttack()
    {
        _isAttacking = false;
        _animator.SetBool("isAttacking", _isAttacking);
    }

    //private
    void Attack()
    {
        if(Input.GetKeyDown(KeyCode.Space) && !_isAttacking)
        {
            _isAttacking = true;
            _animator.SetBool("isAttacking", _isAttacking);
        }
    }
    #endregion
}

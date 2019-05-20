using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            if(health == 0f)
            {
                killEnemy();
            }
        }
    }
    [SerializeField] float health;
    
    void killEnemy()
    {
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    bool playerTargeted = false;
    public float moveSpeed, targetRange;
    Transform target;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerTargeted && Vector2.Distance(transform.position, target.position) <= targetRange)
        {
            playerTargeted = true;
            StartCoroutine(chasePlayer());
        }
    }

    IEnumerator chasePlayer()
    {
        while(playerTargeted)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            transform.right = target.position - transform.position; //Look at target
            if(Vector2.Distance(transform.position, target.position) > targetRange)
            {
                playerTargeted = false;
            }
            yield return null;
        }
    }
}

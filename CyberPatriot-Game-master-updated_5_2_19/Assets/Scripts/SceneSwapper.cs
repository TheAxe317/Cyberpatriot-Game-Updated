using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapper : MonoBehaviour
{

	public int sceneIndex;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag.Equals("Player"))
		{
			SceneManager.LoadScene(sceneIndex);
		}
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public Transform playerTransform;

    void LateUpdate()
    {
		transform.position = playerTransform.position + new Vector3(0f, 0f, -10f);
    }
}

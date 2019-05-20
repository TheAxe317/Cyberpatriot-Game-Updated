using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Polar
{
   
	static public Vector2 RotateByAngle(Vector2 vector, float angle)
	{
		Vector2 newVector = Vector2.zero;

		float radius = Mathf.Sqrt(Mathf.Pow(vector.x, 2f) + Mathf.Pow(vector.y, 2f));
		float theta = Mathf.Atan2(vector.y, vector.x);

		theta += Mathf.Deg2Rad * angle;

		newVector.x = radius * Mathf.Cos(theta);
		newVector.y = radius * Mathf.Sin(theta);

		return newVector;
	}

	static public Vector3 RotateByAngle(Vector3 vector, float angle)
	{
		Vector3 newVector = Vector3.zero;

		float radius = Mathf.Sqrt(Mathf.Pow(vector.x, 2f) + Mathf.Pow(vector.y, 2f));
		float theta = Mathf.Atan2(vector.y, vector.x);

		theta += Mathf.Deg2Rad * angle;

		newVector.x = radius * Mathf.Cos(theta);
		newVector.y = radius * Mathf.Sin(theta);

		return newVector;
	}
}

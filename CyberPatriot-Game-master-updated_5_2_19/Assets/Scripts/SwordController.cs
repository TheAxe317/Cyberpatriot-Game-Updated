using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{

	public Sprite[] swordSprites;

	private ParticleSystem ps;
	private ParticleSystem psAttack;

	private Rigidbody2D rb;

	private bool rotating = false;
	private Vector2 rotateTo;

	public float spinSpeed = 18f;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	public void InitializeIndex(int index)
	{
		GetComponent<SpriteRenderer>().sprite = swordSprites[index];

		ps = transform.GetChild(0).GetComponent<ParticleSystem>();
		ps.textureSheetAnimation.RemoveSprite(0);
		ps.textureSheetAnimation.AddSprite(swordSprites[index]);

		psAttack = transform.GetChild(1).GetComponent<ParticleSystem>();
		psAttack.textureSheetAnimation.RemoveSprite(0);
		psAttack.textureSheetAnimation.AddSprite(swordSprites[index]);
	}

	private void Update()
	{
		var main = ps.main;
		main.startRotation = rb.rotation * -Mathf.Deg2Rad;

		var main2 = psAttack.main;
		main2.startRotation = rb.rotation * -Mathf.Deg2Rad;
	}

	public void AttackEffect(Vector2 location, bool rotate)
	{
		psAttack.Play();

		if (rotate)
		{
			rotateTo = location;
			rotating = true;
			Invoke("StopRotating", 0.35f);
		}
	}

	public void StopAttackEffect()
	{
		psAttack.Stop();
	}

	void StopRotating()
	{
		rotating = false;
	}

	private void FixedUpdate()
	{
		if (rotating)
		{
			float rotationAngle = Mathf.Atan2(rotateTo.y - transform.position.y, rotateTo.x - transform.position.x) * Mathf.Rad2Deg - 90f;
			rb.rotation = Mathf.LerpAngle(rb.rotation, rotationAngle, spinSpeed);
		}
	}

}

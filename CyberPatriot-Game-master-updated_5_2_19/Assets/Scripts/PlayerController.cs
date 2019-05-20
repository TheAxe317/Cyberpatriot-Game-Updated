using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

	[Header("Player Movement")]
	public float moveSpeed;
	public float turnSpeed;

	[Space]

	[Header("Sword Settings")]
	[Range(0, 29)] public int swordCount = 4;
	public SwordType swordType;

	[Space]

	public float swordCooldown;
	public float megaSwordCooldown;

	public float shootRate;
	public float shootSpeed;

	public float megaShootSpeed;

	[Space]

	[Header("Sword Behavior")]
	public float swordIdleSpeed;
	public float swordIdleSpin;
	public float swordIdleDrag;

	[Space]

	public float megaSwordSpeed;
	public float megaSwordSpin;
	public float megaSwordDrag;

	[Space]

	public GameObject swordPrefabBrawler;
	public GameObject swordPrefabFire;
	public GameObject swordPrefabIce;
	public GameObject swordPrefabPsychic;

	private GameObject swordPrefab;

	private Camera cam;
	private float swordRadius;

	private Rigidbody2D rb;
	private Transform swordParent;

	private GameObject[] swords;
	private Vector3[] swordOffsets;

	private Vector3[] megaSwordOffsets;
	private float[] megaSwordAngle;
	private float[] megaSwordSpeedMult;

	private Rigidbody2D[] swordrbs;
	private Collider2D[] swordColls;
	private SpriteRenderer[] swordsrs;

	private SwordController[] swordControllers;

	private bool[] swordIdle;
	private bool[] swordCanShoot;

	private float nextShootTime = 0f;
	private int swordShootIndex = 0;

	private bool megaSword = false;
	private Vector3 megaSwordBase;
	private Vector2 megaSwordTarget;

	private List<int> swordIndexesTaken = new List<int>();

	private bool fire = false;

	private readonly int maxSwords = 29;

	private bool firstShot = true;

	//Fog of War
	private SpriteRenderer fogSR;
	private SpriteMask[] allRoomMasks;

	private Collider2D myColl;

	public enum SwordType { Brawler, Fire, Ice, Psychic }

	void Start()
	{
		Initialize();

		SpawnSwords();
	}

	void Initialize()
	{
		rb = GetComponent<Rigidbody2D>();
		swordParent = GameObject.FindWithTag("SwordParent").transform;
		cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		myColl = GetComponent<Collider2D>();

		//GameObjects
		swords = new GameObject[maxSwords];

		if (swordType == SwordType.Brawler)
		{
			swordPrefab = swordPrefabBrawler;
		} 
		else if (swordType == SwordType.Fire)
		{
			swordPrefab = swordPrefabFire;
		}
		else if (swordType == SwordType.Ice)
		{
			swordPrefab = swordPrefabIce;
		}
		else if (swordType == SwordType.Psychic)
		{
			swordPrefab = swordPrefabPsychic;
		}

		//Positional Info
		swordOffsets = new Vector3[maxSwords];

		megaSwordOffsets = new Vector3[maxSwords];
		megaSwordAngle = new float[maxSwords];
		megaSwordSpeedMult = new float[maxSwords];

		//Components
		swordrbs = new Rigidbody2D[maxSwords];
		swordColls = new Collider2D[maxSwords];
		swordsrs = new SpriteRenderer[maxSwords];

		swordControllers = new SwordController[maxSwords];

		//Booleans
		swordIdle = new bool[maxSwords];
		swordCanShoot = new bool[maxSwords];

		//Fog of War
		fogSR = GameObject.FindGameObjectWithTag("FogOfWar").GetComponent<SpriteRenderer>();
		fogSR.enabled = true;

		GameObject[] allRooms = GameObject.FindGameObjectsWithTag("Room");
		allRoomMasks = new SpriteMask[allRooms.Length];

		for (int i = 0; i < allRoomMasks.Length; i++)
		{
			allRoomMasks[i] = allRooms[i].GetComponent<SpriteMask>();
		}
	}

	private void Update()
	{
		if (rb.velocity.sqrMagnitude >= 0.1f)
		{
			rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, Vector2.SignedAngle(Vector2.up, rb.velocity), turnSpeed));
		}

		if (Input.GetMouseButton(0) && Time.time >= nextShootTime)
		{
			StartCoroutine(ShootSword());
		}

		if (Input.GetMouseButton(1) && Time.time >= nextShootTime)
		{
			StartCoroutine(ShootMegaSword());
		}

		if (Input.GetMouseButton(0))
		{
			fire = false;
		}
		else
		{
			fire = true;
			firstShot = true;
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			AddSwords(1);
		}
	}

	void FixedUpdate()
	{
		BasicMovement();

		if (megaSword)
		{
			FormMegaSword();
		}
		else
		{
			CorrectSwords();
		}
	}

	void BasicMovement()
	{
		Vector2 moveVector = Vector2.zero;

		if (Input.GetKey(KeyCode.W))
		{
			moveVector += Vector2.up;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			moveVector += Vector2.down;
		}

		if (Input.GetKey(KeyCode.A))
		{
			moveVector += Vector2.left;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			moveVector += Vector2.right;
		}

		rb.AddForce(moveVector.normalized * moveSpeed);
	}

	void OnTriggerStay2D(Collider2D coll)
	{
		if (coll.tag.Equals("Room"))
		{
			foreach (SpriteMask sm in allRoomMasks)
			{
				sm.enabled = false;
			}

			coll.GetComponent<SpriteMask>().enabled = true;

			fogSR.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
		}
	}

	void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.tag.Equals("Room"))
		{
			coll.GetComponent<SpriteMask>().enabled = false;

			fogSR.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

			if (!myColl.IsTouchingLayers(LayerMask.GetMask("Room")))
			{
				foreach (SpriteMask sm in allRoomMasks)
				{
					sm.enabled = true;
				}
			}
		}
	}

	#region Swords

	void SpawnSwords()
	{
		swordRadius = 1.15f + 0.01f * swordCount;

		for (int i = 0; i < swordCount; i++)
		{
			float arcAngle = ((Mathf.PI * 1.25f) / (swordCount + 1)) * (i + 1);

			float layerMult = 1f;

			if (swordCount >= 9)
			{
				layerMult = (i % 2) * 0.45f + 1f;
			}

			swordOffsets[i] = new Vector3(Mathf.Cos(arcAngle - Mathf.PI * 0.125f), -Mathf.Sin(arcAngle - Mathf.PI * 0.125f)) * swordRadius * layerMult;

			Vector3 spawnPos = transform.position + swordOffsets[i];
			Quaternion spawnAngle = Quaternion.Euler(0f, 0f, rb.rotation);

			bool alreadyExists = (swords[i] != null);

			if (!alreadyExists)
			{
				swords[i] = Instantiate(swordPrefab, spawnPos, spawnAngle, swordParent);


				swordrbs[i] = swords[i].GetComponent<Rigidbody2D>();
				swordColls[i] = swords[i].GetComponent<Collider2D>();
				swordsrs[i] = swords[i].GetComponent<SpriteRenderer>();

				swordControllers[i] = swords[i].GetComponent<SwordController>();

				/*

				int currentIndex = Random.Range(0, swordControllers[i].swordSprites.Length);
				int safeCount = 0;
				while (swordIndexesTaken.Contains(currentIndex) && safeCount < 100)
				{
					currentIndex = Random.Range(0, swordControllers[i].swordSprites.Length);
					safeCount++;
				}
				swordIndexesTaken.Add(currentIndex);

				*/

				int currentIndex = i;
				if (currentIndex > 5)
				{
					currentIndex = 5;
				}

				swordControllers[i].InitializeIndex(currentIndex);
			}

			swordControllers[i].spinSpeed = swordIdleSpin * 1.5f;
			swordIdle[i] = true;
			swordCanShoot[i] = true;

			//Mega-sword
			if (swordCount == 4)
			{
				if (i == 0)
				{
					megaSwordOffsets[i] = new Vector3(0f, -2f);
					megaSwordAngle[i] = 0f;

					megaSwordSpeedMult[i] = 1f;
				}
				else if (i == 1)
				{
					megaSwordOffsets[i] = new Vector3(0f, -1f);
					megaSwordAngle[i] = 0f;

					megaSwordSpeedMult[i] = 1.25f;
				}
				else if (i == 2)
				{
					megaSwordOffsets[i] = new Vector3(0.2f, -2.5f);
					megaSwordAngle[i] = -60f;

					megaSwordSpeedMult[i] = 0.75f;
				}
				else if (i == 3)
				{
					megaSwordOffsets[i] = new Vector3(-0.2f, -2.5f);
					megaSwordAngle[i] = 60f;

					megaSwordSpeedMult[i] = 0.75f;
				}

			}
		}
	}

	void AddSwords(int num)
	{
		swordCount = Mathf.Clamp(swordCount + num, 0, maxSwords);
		SpawnSwords();
	}

	IEnumerator ShootSword()
	{
		nextShootTime = Time.time + shootRate;
		if (firstShot)
		{
			nextShootTime += shootRate * 1f;
			firstShot = false;
		}

		fire = false;

		if (swordIdle[swordShootIndex] && swordCanShoot[swordShootIndex])
		{
			int currentSword = swordShootIndex;

			swordShootIndex++;
			if (swordShootIndex == swordCount)
			{
				swordShootIndex = 0;
			}

			//Shoot this sword
			swordIdle[currentSword] = false;
			swordCanShoot[currentSword] = false;
			swordControllers[currentSword].AttackEffect(cam.ScreenToWorldPoint(Input.mousePosition), true);
			swordrbs[currentSword].drag *= 2f;

			yield return new WaitUntil(() => fire == true);

			yield return new WaitForSeconds(0.2f);

			//Attack

			swordrbs[currentSword].drag = 0f;
			swordrbs[currentSword].AddForce(swords[currentSword].transform.up * shootSpeed * 5f, ForceMode2D.Impulse);

			swordColls[currentSword].enabled = true;
			swordsrs[currentSword].color = new Color(1f, 1f, 1f, 1f);

			yield return new WaitForSeconds(0.3f);

			swordrbs[currentSword].drag = 7f;

			yield return new WaitForSeconds(0.3f);

			swordrbs[currentSword].drag = swordIdleDrag;
			swordIdle[currentSword] = true;
			swordColls[currentSword].enabled = false;
			swordsrs[currentSword].color = new Color(1f, 1f, 1f, 0.6f);
			swordControllers[currentSword].StopAttackEffect();

			yield return new WaitForSeconds(swordCooldown);

			swordCanShoot[currentSword] = true;
		}
	}

	IEnumerator ShootMegaSword()
	{
		bool canMega = (swordCount == 4);
		for (int i = 0; i < 4; i++)
		{
			if (!swordIdle[i] || !swordCanShoot[i])
			{
				canMega = false;
			}
		}

		if (canMega)
		{
			//Set target and base of megasword
			megaSwordBase = transform.position;
			megaSwordTarget = cam.ScreenToWorldPoint(Input.mousePosition);

			ChangeSwordFormation(true);

			for (int i = 0; i < 4; i++)
			{

				swordCanShoot[i] = false;
			}

			//Getting in formation

			yield return new WaitForSeconds(0.6f);

			for (int i = 0; i < 4; i++)
			{
				swordControllers[i].AttackEffect(megaSwordTarget, false);
			}

			yield return new WaitForSeconds(0.6f);

			Vector2 forceDir = (megaSwordTarget - new Vector2(megaSwordBase.x, megaSwordBase.y)).normalized;

			for (int i = 0; i < 4; i++)
			{
				//Turn off mega formation but also shoot
				swordIdle[i] = false;
				ChangeSwordFormation(false);

				swordColls[i].enabled = true;
				swordsrs[i].color = new Color(1f, 1f, 1f, 1f);

				swordrbs[i].drag = 0f;
				swordrbs[i].AddForce(forceDir * megaShootSpeed * 5f, ForceMode2D.Impulse);


			}

			yield return new WaitForSeconds(0.3f);

			for (int i = 0; i < 4; i++)
			{

				//Apply drag
				swordrbs[i].drag = 7f;

			}

			yield return new WaitForSeconds(0.3f);

			for (int i = 0; i < 4; i++)
			{
				//Revert back to regular swords
				swordrbs[i].drag = swordIdleDrag;
				swordIdle[i] = true;
				swordColls[i].enabled = false;
				swordsrs[i].color = new Color(1f, 1f, 1f, 0.6f);
				swordControllers[i].StopAttackEffect();
			}

			yield return new WaitForSeconds(megaSwordCooldown);

			for (int i = 0; i < 4; i++)
			{
				swordCanShoot[i] = true;
			}
		}
	}

	void ChangeSwordFormation(bool mega)
	{
		megaSword = mega;
		if (megaSword)
		{
			for (int i = 0; i < 4; i++)
			{
				swordrbs[i].drag = megaSwordDrag;
			}
		}
		else
		{
			for (int i = 0; i < 4; i++)
			{
				swordrbs[i].drag = swordIdleDrag;
			}
		}
	}

	void CorrectSwords()
	{
		Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
		float posAngle = Vector2.SignedAngle(Vector2.up, mousePos - new Vector2(transform.position.x, transform.position.y));

		for (int i = 0; i < swordCount; i++)
		{

			if (swordIdle[i])
			{
				Vector3 goalPos = transform.position + Polar.RotateByAngle(swordOffsets[i], posAngle);

				Vector2 moveVector = Vector2.zero;

				if (swords[i].transform.position.x > goalPos.x)
				{
					moveVector += Vector2.left;
				}
				if (swords[i].transform.position.x < goalPos.x)
				{
					moveVector += Vector2.right;
				}

				if (swords[i].transform.position.y > goalPos.y)
				{
					moveVector += Vector2.down;
				}
				if (swords[i].transform.position.y < goalPos.y)
				{
					moveVector += Vector2.up;
				}

				moveVector.Normalize();

				float dist = Mathf.Clamp((swords[i].transform.position - goalPos).magnitude, 0f, 5f);
				if (dist >= 0.1f)
				{
					swordrbs[i].AddForce(moveVector * swordIdleSpeed * dist);
				}

				float rotationAngle = Mathf.Atan2(mousePos.y - swords[i].transform.position.y, mousePos.x - swords[i].transform.position.x) * Mathf.Rad2Deg - 90f;
				swordrbs[i].rotation = Mathf.LerpAngle(swordrbs[i].rotation, rotationAngle, swordIdleSpin);
			}
		}
	}

	void FormMegaSword()
	{
		float posAngle = Vector2.SignedAngle(Vector2.up, megaSwordTarget - new Vector2(transform.position.x, transform.position.y));

		for (int i = 0; i < swordCount; i++)
		{
			if (swordIdle[i])
			{
				Vector3 goalPos = megaSwordBase + Polar.RotateByAngle(megaSwordOffsets[i], posAngle);

				Vector2 moveVector = Vector2.zero;

				if (swords[i].transform.position.x > goalPos.x)
				{
					moveVector += Vector2.left;
				}
				if (swords[i].transform.position.x < goalPos.x)
				{
					moveVector += Vector2.right;
				}

				if (swords[i].transform.position.y > goalPos.y)
				{
					moveVector += Vector2.down;
				}
				if (swords[i].transform.position.y < goalPos.y)
				{
					moveVector += Vector2.up;
				}

				moveVector.Normalize();

				float dist = Mathf.Clamp((swords[i].transform.position - goalPos).magnitude, 0f, 5f);
				if (dist >= 0.1f)
				{
					swordrbs[i].AddForce(moveVector * megaSwordSpeed * dist * megaSwordSpeedMult[i]);
				}


				float rotationAngle = posAngle + megaSwordAngle[i];
				swordrbs[i].rotation = Mathf.LerpAngle(swordrbs[i].rotation, rotationAngle, megaSwordSpin);
			}
		}
	}

	#endregion
}

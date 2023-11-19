using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
	public enum ForceDirection
	{
		Up,
		Down,
		None
	}

	public enum RotationDirection
	{
		Right,
		Left,
		None
	}

	public delegate void PlayerDestroy();
	public event PlayerDestroy OnPlayerDestroy;
	public bool InDeathAnimation { get; private set; } = false;

	[Tooltip("If none is set the player will instantly disappear to then respawn. If an animation is set it delays for the length of the animation.")]
	[SerializeField] private AnimationClip deathAnimationClip;
	[SerializeField]
	private SpriteRenderer playerSpriteRenderer = default;

	[Header("Movement Settings")]

	[SerializeField] 
	private bool shipBackwardsAllowed = false;
	
	[SerializeField]
	private float mainForce = 1000f;

	[SerializeField]
	private float rotationForce = 200f;

	[SerializeField]
	private float maxAxisVelocity = 42f;

	[Header("Destruction Settings")]
	[SerializeField] 
	private bool destroyOnCollision = true;

	[SerializeField]
	private GameObject objectToSpawnOnDestruction;

	[Header("Objects to enable on Collision")]
	[SerializeField]
	private GameObject objectToEnableAt3HP;
	[SerializeField]
	private GameObject objectToEnableAt2HP;
	[SerializeField]
	private GameObject objectToEnableAt1HP;

	[Tooltip("Time until the time spawned object is destroyed. Negative Values mean it will not be destroyed.")]
	[SerializeField]
	private float timeUntilDestroySpawnedObject = -1f;

	[SerializeField]
	private AudioClip destroyAudioClip = default;

	[SerializeField]
	private float respawnInvulnerableTime = 3f; // In seconds

	[SerializeField]
	private int InvulnerableBlinkNumber = 3;
	
	[Header("Setup")]

	[SerializeField] 
	private Animator anim;

	private Rigidbody2D playerRigidbody2D;

	private bool IsInvulnerable { get; set; }

	[SerializeField]
	private Game game;
	

	private void Awake()
	{
		if(objectToEnableAt3HP != null) objectToEnableAt3HP.SetActive(true); 
		if(objectToEnableAt2HP != null) objectToEnableAt2HP.SetActive(false); 
		if(objectToEnableAt1HP != null) objectToEnableAt1HP.SetActive(false); 
			
		playerRigidbody2D = GetComponent<Rigidbody2D>();

		IsInvulnerable = false;

		if (game == null)
		{
			GameObject gameHolder = null;
			gameHolder = GameObject.Find("Game");
			if (gameHolder != null)
			{
				game = gameHolder.GetComponent<Game>();
			}
			else
			{
				Debug.LogError("Did not find a 'Game' component in your scene!");
			}
		}
		
		if (anim == null) anim = GetComponent<Animator>();

		ShootBehavior.BulletShotEvent += OnBulletShot;
	}

	private void OnDestroy()
	{
		ShootBehavior.BulletShotEvent -= OnBulletShot;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer != LayerMask.NameToLayer("Asteroid") || InDeathAnimation) return;
		if (IsInvulnerable == false)
		{
			if (destroyOnCollision || game.CurrentPlayerLifes == 1)
			{
				StartCoroutine(DestroyPlayer());
			}
			else
			{
				OnPlayerDestroy?.Invoke();
				StartCoroutine(TemporaryInvulnerability());
			}
		}
	}

	public void Respawn()
	{
		anim.ResetTrigger("Death");
		anim.ResetTrigger("Shoot");
		
		switch (game.CurrentPlayerLifes)
		{
			case 3:
				if(objectToEnableAt3HP != null) objectToEnableAt3HP.SetActive(true); 
				if(objectToEnableAt2HP != null) objectToEnableAt2HP.SetActive(false); 
				if(objectToEnableAt1HP != null) objectToEnableAt1HP.SetActive(false);
				break;
			case 2:
				if(objectToEnableAt3HP != null) objectToEnableAt3HP.SetActive(false); 
				if(objectToEnableAt2HP != null) objectToEnableAt2HP.SetActive(true); 
				if(objectToEnableAt1HP != null) objectToEnableAt1HP.SetActive(false);
				break;
			case 1:
				if(objectToEnableAt3HP != null) objectToEnableAt3HP.SetActive(false); 
				if(objectToEnableAt2HP != null) objectToEnableAt2HP.SetActive(false); 
				if(objectToEnableAt1HP != null) objectToEnableAt1HP.SetActive(true);
				break;
				
		}
		if (!destroyOnCollision) return;
		transform.position = new Vector3(0f, 0f, transform.position.z);
		transform.rotation = Quaternion.identity;
		gameObject.SetActive(true);
		StartCoroutine(TemporaryInvulnerability());
	}

	public void AddRelativeForce(ForceDirection forceDirection)
	{

		Vector2 forceDirectionVector = Vector2.zero;
		anim.SetFloat("CurrentSpeed", playerRigidbody2D.velocity.magnitude);

		switch (forceDirection)
		{
			case ForceDirection.Up:
				forceDirectionVector = Vector2.up;
				anim.SetFloat("InputSpeed", 1f);
				break;
			case ForceDirection.Down:
				if (shipBackwardsAllowed)
				{
					forceDirectionVector = Vector2.down;
					anim.SetFloat("InputSpeed", -1f);
				}
				else
				{
					anim.SetFloat("InputSpeed", 0f);
					return;
				}
				break;
			case ForceDirection.None:
				anim.SetFloat("InputSpeed", 0f);
				return;
		}

		playerRigidbody2D.AddRelativeForce(mainForce * Time.deltaTime * forceDirectionVector);

		if (Mathf.Abs(playerRigidbody2D.velocity.x) > maxAxisVelocity)
		{
			float newVelocity = (playerRigidbody2D.velocity.x > 0) ? maxAxisVelocity : -1 * maxAxisVelocity;
			playerRigidbody2D.velocity = new Vector2(newVelocity, playerRigidbody2D.velocity.y);
		}

		if (Mathf.Abs(playerRigidbody2D.velocity.y) > maxAxisVelocity)
		{
			float newVelocity = (playerRigidbody2D.velocity.y > 0) ? maxAxisVelocity : -1 * maxAxisVelocity;
			playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, newVelocity);
		}
		
		anim.SetFloat("CurrentSpeed", playerRigidbody2D.velocity.magnitude);
	}

	public void AddRotation(RotationDirection rotationDirection)
	{
		float rotationDirectionValue = 0f;
		switch (rotationDirection)
		{
			case RotationDirection.Right:
				rotationDirectionValue = -1f;
				break;
			case RotationDirection.Left:
				rotationDirectionValue = 1f;
				break;
			case RotationDirection.None:
				rotationDirectionValue = 0f;
				break;
		}

		anim.SetFloat("Rotation", rotationDirectionValue);

		playerRigidbody2D.rotation += rotationForce * Time.deltaTime * rotationDirectionValue;
	}

	private IEnumerator DestroyPlayer()
	{
		if (destroyAudioClip != null)
		{
			AudioManager.Instance.EffectsAudioSource.PlayOneShot(destroyAudioClip);
		}

		if(objectToSpawnOnDestruction != null)
		{
			GameObject spawnedObject = Instantiate(objectToSpawnOnDestruction, transform.position, Quaternion.identity);

			if (timeUntilDestroySpawnedObject >= 0)
			{
				Destroy(spawnedObject,timeUntilDestroySpawnedObject);
			}
		}

		anim.SetTrigger("Death");

		InDeathAnimation = true;
		yield return new WaitForSeconds(deathAnimationClip == null ? 0f : deathAnimationClip.length);
		InDeathAnimation = false;

		OnPlayerDestroy?.Invoke();
		gameObject.SetActive(false);
	}

	private IEnumerator TemporaryInvulnerability()
	{
		IsInvulnerable = true;

		Color spriteColor = playerSpriteRenderer.color;
		float originalAlpha = spriteColor.a;

		float blinkHalfPeriod = respawnInvulnerableTime / (InvulnerableBlinkNumber * 2);
		for (int i = 0; i < InvulnerableBlinkNumber; i++)
		{
			yield return new WaitForSeconds(blinkHalfPeriod);

			spriteColor.a = 0f;
			playerSpriteRenderer.color = spriteColor;

			yield return new WaitForSeconds(blinkHalfPeriod);

			spriteColor.a = originalAlpha;
			playerSpriteRenderer.color = spriteColor;
		}

		IsInvulnerable = false;
	}

	private void OnBulletShot()
	{
		anim.SetTrigger("Shoot");
	}
}

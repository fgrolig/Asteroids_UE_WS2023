﻿using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInputController : MonoBehaviour
{
	[SerializeField]
	private ShootBehavior shootingSource = default;

	private Player player;

	private void Awake()
	{
		player = GetComponent<Player>();
	}

	void Update()
	{
		if (player.isActiveAndEnabled && Game.CurrentGameState == Game.GameState.Active && !player.InDeathAnimation)
		{
			if (Input.GetAxisRaw("Vertical") > 0)
			{
				player.AddRelativeForce(Player.ForceDirection.Up);
			}

			if (Input.GetAxisRaw("Vertical") == 0)
			{
				player.AddRelativeForce(Player.ForceDirection.None);
			}
			if (Input.GetAxisRaw("Horizontal") > 0)
			{
				player.AddRotation(Player.RotationDirection.Right);
			}

			if (Input.GetAxisRaw("Horizontal") == 0)
			{
				player.AddRotation(Player.RotationDirection.None);
			}
			if (Input.GetAxisRaw("Vertical") < 0)
			{
				player.AddRelativeForce(Player.ForceDirection.Down);
			}
			if (Input.GetAxisRaw("Horizontal") < 0)
			{
				player.AddRotation(Player.RotationDirection.Left);
			}
			if (Input.GetButtonDown("Fire1"))
			{
				shootingSource.Shoot();
			}
		}
	}
}

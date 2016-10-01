using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpShield : IPowerUp
{
	protected override string Trigger(ShipController instance)
	{
		instance.SetShieldEnabled(true);
		return "Shield";
	}
}

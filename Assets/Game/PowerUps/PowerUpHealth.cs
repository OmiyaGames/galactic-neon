using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpHealth : IPowerUp
{
	public int health = 3;

	protected override string Trigger(ShipController instance)
	{
		instance.Life += health;
		return "+Health";
	}
}

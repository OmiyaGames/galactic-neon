using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpScore : IPowerUp
{
	public int score = 3;

	protected override string Trigger(ShipController instance)
	{
		instance.IncrementScore(score);
		return "Score +" + score;
	}
}

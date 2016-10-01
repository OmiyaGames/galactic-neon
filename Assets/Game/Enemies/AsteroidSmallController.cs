using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class AsteroidSmallController : IEnemy
{
	public Vector2 torqueRange = new Vector2(-10, 10);

	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;

	// Use this for initialization
	void Start ()
	{
		// Randomize color
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.color = IPowerUp.FromHSV(Random.value, IEnemy.saturation, IEnemy.brightness);

		if(ShipController.Instance != null)
		{
			ShipController.Instance.AddDestructable(this);

			// Randomize torque
			GetComponent<Rigidbody2D>().angularVelocity = Random.Range(torqueRange.x, torqueRange.y);
			
			// Reset life
			currentLife = 0;
		}
	}
}

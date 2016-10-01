using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class AsteroidSmallController : IEnemy
{
    [Header("Asteroid Small")]
	public Vector2 torqueRange = new Vector2(-10, 10);

	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Randomize color
		mainRenderer.color = IPowerUp.FromHSV(Random.value, IEnemy.saturation, IEnemy.brightness);

		if(IGameManager.Instance != null)
		{
			IGameManager.Instance.AddDestructable(this);

			// Randomize torque
			rigidbody2D.angularVelocity = Random.Range(torqueRange.x, torqueRange.y);
		}
	}
}

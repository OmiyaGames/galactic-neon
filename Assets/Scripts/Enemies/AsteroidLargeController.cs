using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class AsteroidLargeController : IEnemy
{
	[System.Serializable]
	public class SpawnParams
	{
		public float aimRange = 10f;
		public Vector2 speedRange = new Vector2(1, 20);
		public Vector2 torqueRange = new Vector2(-10, 10);
	}
	[System.Serializable]
	public class SmallAsteroidSet
	{
		public AsteroidSmallController spawnAsteroid = null;
		public Transform[] spawnPositions;
		public Vector2 speedRange = new Vector2(1, 5);
	}

    [Header("Asteroid Large")]
    public SpawnParams spawnParams;
    public SmallAsteroidSet smallerAsteroids;

	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;

	public override void Destroy(MonoBehaviour controller)
	{
		// Check if we have smaller asteroids
		if((controller is BulletController) && (smallerAsteroids.spawnAsteroid != null) && (smallerAsteroids.spawnPositions != null))
		{
			// Start cloning small asteroids
			GameObject clone = null;
			Vector2 velocity;
			for(int index = 0; index < smallerAsteroids.spawnPositions.Length; ++index)
			{
				if(smallerAsteroids.spawnPositions[index] != null)
				{
					// Clone the asteroid
					clone = GlobalGameObject.Get<PoolingManager>().GetInstance(smallerAsteroids.spawnAsteroid.gameObject, smallerAsteroids.spawnPositions[index].position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
					
					// Calculate asteroids trajectory
					velocity.x = smallerAsteroids.spawnPositions[index].position.x - transform.position.x;
					velocity.y = smallerAsteroids.spawnPositions[index].position.y - transform.position.y;
					velocity.Normalize();
					velocity *= Random.Range(smallerAsteroids.speedRange.x, smallerAsteroids.speedRange.y);
					clone.rigidbody2D.velocity = velocity;
				}
			}
		}
		base.Destroy(controller);
	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Randomize color
		mainRenderer.color = IPowerUp.FromHSV(Random.value, IEnemy.saturation, IEnemy.brightness);

		if((ShipController.Instance != null) && (IGameManager.Instance != null))
		{
			// Randomize torque
			rigidbody2D.angularVelocity = Random.Range(spawnParams.torqueRange.x, spawnParams.torqueRange.y);

			if(IGameManager.Instance is ObjectSpawner)
			{
				// Calculate where to aim for
				mSpawnPosition = ShipController.Instance.transform.position;
				mRandomOffset = Random.insideUnitCircle.normalized * spawnParams.aimRange;
				mSpawnPosition.x += mRandomOffset.x;
				mSpawnPosition.y += mRandomOffset.y;
				mSpawnPosition -= transform.position;
				mSpawnPosition.Normalize();
				
				// Randomize force
				mSpawnPosition *= Random.Range(spawnParams.speedRange.x, spawnParams.speedRange.y);
				rigidbody2D.velocity = mSpawnPosition;
			}
		}
	}
}

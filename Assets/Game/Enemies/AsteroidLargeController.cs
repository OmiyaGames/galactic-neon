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
	public SpawnParams spawnParams;
	[System.Serializable]
	public class SmallAsteroidSet
	{
		public AsteroidSmallController spawnAsteroid = null;
		public Transform[] spawnPositions;
		public Vector2 speedRange = new Vector2(1, 5);
	}
	public SmallAsteroidSet smallerAsteroids;

	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;
	private IDestructable clonedScript = null;

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
					clone = (GameObject)Instantiate(smallerAsteroids.spawnAsteroid.gameObject, smallerAsteroids.spawnPositions[index].position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
					
					// Calculate asteroids trajectory
					velocity.x = smallerAsteroids.spawnPositions[index].position.x - transform.position.x;
					velocity.y = smallerAsteroids.spawnPositions[index].position.y - transform.position.y;
					velocity.Normalize();
					velocity *= Random.Range(smallerAsteroids.speedRange.x, smallerAsteroids.speedRange.y);
					clone.GetComponent<Rigidbody2D>().velocity = velocity;
				}
			}
		}
		base.Destroy(controller);
	}

	public override void Preload(bool state)
	{
		if(state == false)
		{
			GameObject clone = (GameObject)Instantiate(smallerAsteroids.spawnAsteroid.gameObject, transform.position, Quaternion.identity);
			clonedScript = clone.GetComponent<IDestructable>();
		}
		if(clonedScript != null)
		{
			clonedScript.Preload(state);
		}
		base.Preload(state);
	}

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
			GetComponent<Rigidbody2D>().angularVelocity = Random.Range(spawnParams.torqueRange.x, spawnParams.torqueRange.y);

			// Calculate where to aim for
			mSpawnPosition = ShipController.Instance.transform.position;
			mRandomOffset = Random.insideUnitCircle.normalized * spawnParams.aimRange;
			mSpawnPosition.x += mRandomOffset.x;
			mSpawnPosition.y += mRandomOffset.y;
			mSpawnPosition -= transform.position;
			mSpawnPosition.Normalize();
			
			// Randomize force
			mSpawnPosition *= Random.Range(spawnParams.speedRange.x, spawnParams.speedRange.y);
			GetComponent<Rigidbody2D>().velocity = mSpawnPosition;

			// Reset life
			currentLife = 0;
		}
	}
}

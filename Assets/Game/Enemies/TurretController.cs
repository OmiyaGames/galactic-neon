using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class TurretController : IEnemy
{
	public enum Type
	{
		Simple,
		Target,
		MoveTarget
	}
	public SpriteRenderer baseRenderer;
	public Type targetShip = Type.Simple;
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
		public float speed = 5;
		public float gapBetweenAsteroids = 1f;
		public ParticleSystem[] gunParticles = null;
	}
	public SmallAsteroidSet smallerAsteroids;
	[System.Serializable]
	public class DetectionParams
	{
		public float detectRange = 15f;
		public float moveForce = 10f;
		public float turnForceMultiplier = 10f;
		public float maxTurnForce = 10f;
		public ParticleSystem jetParticles = null;
		public AudioSource jetSound = null;
	}
	public DetectionParams detection;

	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;
	private float lastFired = 0;
	private bool isShipWithinRange = false;
	private GameObject clone = null;
	private Vector2 velocity;
	private int index = 0;
	private float targetAngle;
	
	// Use this for initialization
	void Start ()
	{
		// Update sprite renderer color
		spriteRenderer = baseRenderer;
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

			// Update when we last fired
			detection.detectRange *= detection.detectRange;
			lastFired = Time.time;
		}
	}

	void Update()
	{
		// Check if the ship is in detection range, and we're ready to fire the weapon
		if((isShipWithinRange == true) && (smallerAsteroids.spawnAsteroid != null) && (smallerAsteroids.spawnPositions != null) && ((Time.time - lastFired) > smallerAsteroids.gapBetweenAsteroids))
		{
			// Start cloning small asteroids
			for(index = 0; index < smallerAsteroids.spawnPositions.Length; ++index)
			{
				if(smallerAsteroids.spawnPositions[index] != null)
				{
					// Clone the asteroid
					clone = (GameObject)Instantiate(smallerAsteroids.spawnAsteroid.gameObject, smallerAsteroids.spawnPositions[index].position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
					
					// Calculate asteroids trajectory
					velocity.x = smallerAsteroids.spawnPositions[index].position.x - transform.position.x;
					velocity.y = smallerAsteroids.spawnPositions[index].position.y - transform.position.y;
					velocity.Normalize();
					velocity *= smallerAsteroids.speed;
					clone.GetComponent<Rigidbody2D>().velocity = velocity;
				}
			}

			// Emit the guns
			for(index = 0; index < smallerAsteroids.gunParticles.Length; ++index)
			{
				if(smallerAsteroids.gunParticles[index] != null)
				{
					smallerAsteroids.gunParticles[index].Emit(50);
				}
			}

			// Update time
			lastFired = Time.time;
		}
	}

	void FixedUpdate()
	{
		// Get the distance from this object to the ship
		mRandomOffset = ShipController.Instance.transform.position;
		mRandomOffset.x -= transform.position.x;
		mRandomOffset.y -= transform.position.y;
		
		// Check if the ship is in detection range
		if(mRandomOffset.sqrMagnitude < detection.detectRange)
		{
			isShipWithinRange = true;
			if(targetShip != Type.Simple)
			{
				// Get the angle we're aiming for
				targetAngle = ShipController.Angle(mRandomOffset);
				
				// Calculate the angular torque
				targetAngle -= transform.rotation.eulerAngles.z;
				targetAngle *= detection.turnForceMultiplier;
				if(targetAngle > detection.maxTurnForce)
				{
					targetAngle = detection.maxTurnForce;
				}
				else if(targetAngle < (detection.maxTurnForce * -1))
				{
					targetAngle = (detection.maxTurnForce * -1);
				}
				
				// Start turning towards the ship
				GetComponent<Rigidbody2D>().AddTorque(targetAngle);

				// Move towards the direction this object is facing
				if(targetShip == Type.MoveTarget)
				{
					GetComponent<Rigidbody2D>().AddForce(transform.up * detection.moveForce);
					if(detection.jetParticles.enableEmission == false)
					{
						detection.jetParticles.enableEmission = true;
						detection.jetSound.Play();
					}
				}
			}
		}
		else
		{
			isShipWithinRange = false;
			if((targetShip == Type.MoveTarget) && (detection.jetParticles.enableEmission == true))
			{
				detection.jetParticles.enableEmission = false;
				detection.jetSound.Stop();
			}
		}
	}
}

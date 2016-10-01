using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class KamikazeController : IEnemy
{
    [Header("Kamikaze")]
	public ParticleSystem jetParticles = null;
	[System.Serializable]
	public class DetectionParams
	{
		public float detectRange = 10f;
		public float moveForce = 1000f;
		public float turnForceMultiplier = 10f;
		public float maxTurnForce = 10f;
	}
	public DetectionParams detection;
	[System.Serializable]
	public class SpawnParams
	{
		public float aimRange = 10f;
		public Vector2 speedRange = new Vector2(1, 20);
	}
	public SpawnParams spawnParams;
	public AudioSource jetSound;

	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;
	private float targetAngle;
	private bool isMoving = false;

	// Use this for initialization
	public override void Start ()
	{
		base.Start();

		// Randomize color
		mainRenderer.color = IPowerUp.FromHSV(Random.value, IEnemy.saturation, IEnemy.brightness);

		if((ShipController.Instance != null) && (IGameManager.Instance != null))
		{
			// Check if we need to fling this enemy towards the player
			if(IGameManager.Instance is ObjectSpawner)
			{
				// Calculate where to aim for
				mSpawnPosition = ShipController.Instance.transform.position;
				mRandomOffset = Random.insideUnitCircle.normalized * spawnParams.aimRange;
				mSpawnPosition.x += mRandomOffset.x;
				mSpawnPosition.y += mRandomOffset.y;
				mSpawnPosition -= transform.position;
				mSpawnPosition.Normalize();
				
				// Face the direction of velocity
				transform.rotation = ShipController.LookRotation2D(mSpawnPosition);

				// Randomize force
				mSpawnPosition *= Random.Range(spawnParams.speedRange.x, spawnParams.speedRange.y);
				rigidbody2D.velocity = mSpawnPosition;
			}

			// Setup detection
			detection.detectRange *= detection.detectRange;
		}
	}

	void FixedUpdate()
	{
		// Get the distance from this object to the ship
		mRandomOffset = ShipController.Instance.transform.position;
		mRandomOffset.x -= transform.position.x;
		mRandomOffset.y -= transform.position.y;

		// Check if the ship is in detection range
		isMoving = (mRandomOffset.sqrMagnitude < detection.detectRange);
		if(isMoving == true)
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
			rigidbody2D.AddTorque(targetAngle);

			// Move towards the direction this object is facing
			rigidbody2D.AddForce(transform.up * detection.moveForce);
		}
		if(jetParticles.enableEmission != isMoving)
		{
			jetParticles.enableEmission = isMoving;
			if(isMoving == true)
			{
				jetSound.Play();
			}
			else
			{
				jetSound.Stop();
			}
		}
	}
}

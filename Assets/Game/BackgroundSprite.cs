using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(Rigidbody2D))]
public class BackgroundSprite : MonoBehaviour
{
	public const float saturation = 0.5f;
	public const float brightness = 1f;
	public float duration = 1f;
	public SpriteRenderer spriteRenderer = null;
	public float maxSpinSpeed = 180f;

	private float maximumDistanceFromShip = 20f;
	private Vector3 position;
	private Vector2 offset;
	private float startTime = -1f;
	private Color spriteColor = Color.white;
	private float rotateSpeed = 0;

	public float StartTime
	{
		get
		{
			return startTime;
		}
	}

	public float MaxDistanceFromShip
	{
		set
		{
			maximumDistanceFromShip = value;
		}
	}

	public float Alpha
	{
		set
		{
			spriteColor.a = value;
		}
	}

	public float RotateSpeed
	{
		get
		{
			return rotateSpeed;
		}
	}

	// Use this for initialization
	public void Start ()
	{
		// Update start time
		startTime = Time.time;

		// Position the star
		position = ShipController.Instance.transform.position;
		offset = Random.insideUnitCircle * maximumDistanceFromShip;
		position.x += offset.x;
		position.y += offset.y;
		position.z = transform.position.z;
		transform.position = position;

		// Cancel physics
		rigidbody2D.velocity = Vector2.zero;
		rigidbody2D.angularVelocity = 0;

		// Rotate star
		transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

		// Update color
		IPowerUp.FromHSV(Random.value, saturation, brightness, ref spriteColor);
		spriteRenderer.color = spriteColor;

		// Update rotate speed
		rotateSpeed = Random.Range(-maxSpinSpeed, maxSpinSpeed);

		// Animate the star
		animation.Play();
	}
}

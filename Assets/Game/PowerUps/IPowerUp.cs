using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class IPowerUp : IDestructable
{
	public const float velocity = 7.5f;
	public const float hueChangeSpeed = 1f;
	public const float saturation = 1f;
	public const float brightness = 1f;

	private SpriteRenderer attachedRenderer;
	private float hue = 0;
	private Color spriteColor = Color.red;
	private Vector3 velocityVector = Vector3.zero;

	public override void Destroy(MonoBehaviour controller)
	{
		Destroy(gameObject);
	}

	public override void Preload(bool state)
	{
		if(state == true)
		{
			Destroy(gameObject);
		}
	}

	protected void Start()
	{
		// Add to destructable
		ShipController.Instance.AddDestructable(this);

		// Get a random hue
		hue = Random.value;
		FromHSV(hue, saturation, brightness, ref spriteColor);

		// Setup the renderer
		attachedRenderer = GetComponent<SpriteRenderer>();
		if(attachedRenderer != null)
		{
			attachedRenderer.color = spriteColor;
		}

		// Move in a random direction
		Vector2 randomVelocity = (Random.insideUnitCircle.normalized * velocity);
		velocityVector.x = randomVelocity.x;
		velocityVector.y = randomVelocity.y;
	}

	// Update is called once per frame
	protected void Update ()
	{
		if(attachedRenderer != null)
		{
			hue += hueChangeSpeed * Time.deltaTime;
			if(hue > 1)
			{
				hue = 0;
			}
			FromHSV(hue, saturation, brightness, ref spriteColor);
			attachedRenderer.color = spriteColor;
		}
	}

	protected void FixedUpdate ()
	{
		GetComponent<Rigidbody2D>().transform.position += (velocityVector * Time.deltaTime);
	}

	protected void OnTriggerEnter2D(Collider2D other)
	{
		if((other != null) && (other.CompareTag("Player") == true))
		{
			ShipController.Instance.PowerUpEffect(Trigger(ShipController.Instance));
			Destroy(ShipController.Instance);
		}
	}

	protected abstract string Trigger(ShipController instance);

	public static Color FromHSV(float hue, float saturation, float brightness)
	{
		Color returnColor = Color.white;
		FromHSV(hue, saturation, brightness, ref returnColor);
		return returnColor;
	}

	public static void FromHSV(float hue, float saturation, float brightness, ref Color color)
	{
		// Normalize values
		if(hue < 0)
		{
			hue = 0;
		}
		else if(hue > 1)
		{
			hue = 1;
		}
		if(saturation < 0)
		{
			saturation = 0;
		}
		else if(saturation > 1)
		{
			saturation = 1;
		}
		if(brightness < 0)
		{
			brightness = 0;
		}
		else if(brightness > 1)
		{
			brightness = 1;
		}

		// Create the new color
		color.r = brightness;
		color.g = brightness;
		color.b = brightness;
		if(saturation > 0)
		{
			// Setup values
			float max = brightness;
			float dif = brightness * saturation;
			float min = brightness - dif;
			hue *= 360;

			// Set to default color
			color.r = 0;
			color.g = 0;
			color.b = 0;

			// Check hue
			if(hue < 60)
			{
				color.r = max;
				color.g = ((hue * dif) / 60) + min;
				color.b = min;
			}
			else if(hue < 120)
			{
				color.r = (((hue - 120) * -dif) / 60) + min;
				color.g = max;
				color.b = min;
			}
			else if(hue < 180)
			{
				color.r = min;
				color.g = max;
				color.b = (((hue - 120) * dif) / 60) + min;
			}
			else if(hue < 240)
			{
				color.r = min;
				color.g = (((hue - 240) * -dif) / 60) + min;
				color.b = max;
			}
			else if(hue < 300)
			{
				color.r = (((hue - 240) * dif) / 60) + min;
				color.g = min;
				color.b = max;
			}
			else if(hue.CompareTo(360) <= 0)
			{
				color.r = max;
				color.g = min;
				color.b = (((hue - 360) * -dif) / 60) + min;
			}
		}
	}
}

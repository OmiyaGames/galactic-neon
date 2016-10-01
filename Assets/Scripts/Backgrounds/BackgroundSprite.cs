using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(Rigidbody2D))]
public class BackgroundSprite : IPooledObject
{
    static readonly HashSet<BackgroundSprite> allBackgroundSprites = new HashSet<BackgroundSprite>();

	public const float saturation = 0.5f;
	public const float brightness = 1f;
	public float duration = 1f;
	public SpriteRenderer spriteRenderer = null;
    public TextMesh textRenderer = null;
	public float maxSpinSpeed = 180f;

	float maximumDistanceFromShip = 20f;
	Vector3 position;
	Vector2 offset;
	float startTime = -1f;
	Color spriteColor = Color.white;
	float rotateSpeed = 0;
    bool deactivateNextLoop = false;
    GameObject objectCache = null;
    Transform transformCache = null;

    public GameObject ThisObject
    {
        get
        {
            if(objectCache == null)
            {
                objectCache = gameObject;
            }
            return objectCache;
        }
    }

    public Transform SpriteTransform
    {
        get
        {
            if(transformCache == null)
            {
                if(spriteRenderer != null)
                {
                    transformCache = spriteRenderer.transform;
                }
                else if(textRenderer != null)
                {
                    transformCache = textRenderer.transform;
                }
            }
            return transformCache;
        }
    }

    public static IEnumerable<BackgroundSprite> AllBackgroundSprites
    {
        get
        {
            return allBackgroundSprites;
        }
    }

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

    public bool DeactivateNextLoop
    {
        get
        {
            return deactivateNextLoop;
        }
    }

	// Use this for initialization
	public override void Start()
	{
        // Add this sprite into the collection
        allBackgroundSprites.Add(this);
        deactivateNextLoop = false;

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
        if(spriteRenderer != null)
        {
    		spriteRenderer.color = spriteColor;
        }
        else if(textRenderer != null)
        {
            textRenderer.color = spriteColor;
        }

		// Update rotate speed
		rotateSpeed = Random.Range(-maxSpinSpeed, maxSpinSpeed);

		// Animate the star
		animation.Play();
	}

    public void Deactivate()
    {
        deactivateNextLoop = true;
    }
}

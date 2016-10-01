using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletControllerHoming : BulletController
{
	public float turnLerpValue = 3f;
	public GameObject reticle = null;

	// Target to home to.  Can be null!
    Transform cacheTransform = null;
	private IEnemy target = null;
    private GameObject targetGameObject = null;
    private Transform targetTransform = null;
    private bool isTargetSet = false;
	private Vector3 currentUpDirection;

	public IEnemy Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
			isTargetSet = true;
			if(target != null)
			{
				target.AddHomingBullet(this);
                targetGameObject = target.gameObject;
                targetTransform = target.transform;
			}
            else
            {
                targetGameObject = null;
                targetTransform = null;
            }
		}
	}

    public override void Initialized(PoolingManager manager)
    {
        base.Initialized(manager);
        cacheTransform = transform;
    }

	public override void Start()
	{
		// Setup bullet
		base.Start();

		// Setup velocity
		currentUpDirection = transform.up;
		velocityVector = (currentUpDirection * velocity);
	}

	void Update()
	{
		if(isTargetSet == false)
		{
			Target = ShipController.Instance.homingTrigger.ClosestEnemy;
		}

        if((target != null) && (targetGameObject.activeSelf == true))
		{
			// Face towards the enemy
            velocityVector = targetTransform.position;
            velocityVector -= cacheTransform.position;
			velocityVector.Normalize();
			currentUpDirection = Vector3.Lerp(currentUpDirection, velocityVector, (Time.deltaTime * turnLerpValue));
			transform.rotation = ShipController.LookRotation2D(currentUpDirection);

			// Setup velocity
			velocityVector = (transform.up * velocity);
		}
        else if(targetGameObject.activeSelf == false)
        {
            Target = null;
        }
	}

	protected override void FixedUpdate ()
	{
		rigidbody2D.velocity = velocityVector;
	}

	public override void Destroy(MonoBehaviour controller)
	{
		// Remove this bullet from the targeted enemy
		if(Target != null)
		{
			Target.RemoveHomingBullet(this);
		}
		base.Destroy(controller);
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class IEnemy : IDestructable
{
	public const float saturation = 0.7f;
	public const float brightness = 1f;

    [Header("Required Components")]
    public SpriteRenderer mainRenderer;
    public Rigidbody2D physicsBody;

    [Header("Stats")]
	public int damage = 1;
	public int maxLife = 1;
	public int score = 1;
    public RandomList powerUps;

    [Header("Special Effects")]
	public Detonator explodeParticle = null;

	protected int currentLife = 0;
	private readonly HashSet<BulletControllerHoming> allHomingBullets = new HashSet<BulletControllerHoming>();
	GameObject homingReticle = null;
	IEnemy originalScript = null;

	#region Properties

	public bool IsTargeted
	{
		get
		{
			return (allHomingBullets.Count > 0);
		}
	}

	public int Life
	{
		get
		{
			return currentLife;
		}
		set
		{
			currentLife = value;
			if(currentLife < 0)
			{
				currentLife = 0;
			}
			else if(currentLife > maxLife)
			{
				currentLife = maxLife;
			}
		}
	}

	public int Score
	{
		get
		{
			return score;
		}
	}

	public IEnemy OriginalScript
	{
		get
		{
			IEnemy returnScript = this;
			if(originalScript != null)
			{
				returnScript = originalScript;
			}
			return returnScript;
		}
	}

	#endregion

	public override void Initialized (PoolingManager manager)
	{
		base.Initialized (manager);
		originalScript = OriginalPrefab.GetComponent<IEnemy>();
        if(mainRenderer == null)
        {
            mainRenderer = GetComponent<SpriteRenderer>();
        }
        if(physicsBody == null)
        {
            physicsBody = GetComponent<Rigidbody2D>();
        }
	}

	public override void Start()
	{
		base.Start();
		Life = maxLife;
	}

	public override void Destroy(MonoBehaviour controller)
	{
		if(explodeParticle != null)
		{
			if((controller is BulletController) || (controller is IPowerUp))
			{
                if((powerUps != null) && (powerUps.NumberOfEntries > 0))
				{
                    GameObject powerUp = powerUps.RandomElement;
					if(powerUp != null)
					{
						GlobalGameObject.Get<PoolingManager>().GetInstance(powerUp.gameObject, transform.position, Quaternion.identity);
					}
				}
				CloneDetonator();
			}
			else if(controller is ShipController)
			{
				CloneDetonator();
			}
		}

		// Deactivate the gameobject, effectively returning this asset back to the pooling manager
		gameObject.SetActive(false);
        allHomingBullets.Clear();
	}
	
	public virtual IEnemyInfo GenerateInfo()
	{
		BasicEnemyInfo returnInfo = new BasicEnemyInfo(OriginalScript, Life, transform.position, transform.rotation);
		returnInfo.GatherRigidBodyInfo(physicsBody);
		returnInfo.GatherRendererInfo(mainRenderer);
		return returnInfo;
	}
	
	public virtual IEnemyInfo GenerateNewInfo(Vector3 position, Quaternion rotation)
	{
		BasicEnemyInfo returnInfo = new BasicEnemyInfo(OriginalScript, position, rotation);
		return returnInfo;
	}

	public virtual int Damage(ShipController controller)
	{
		return damage;
	}
	
	public virtual void Hit(BulletController controller)
	{
        if((Life > 0) && (gameObject.activeSelf == true))
        {
    		Life -= controller.damage;
    		if(Life == 0)
    		{
    			Destroy(controller);
    		}
        }
	}

	public void AddHomingBullet(BulletControllerHoming bullet)
	{
		if(bullet != null)
		{
			allHomingBullets.Add(bullet);
			if(bullet.reticle != null)
			{
				// Grab a reticle from the pool manager
				homingReticle = GlobalGameObject.Get<PoolingManager>().GetInstance(bullet.reticle);
				
				// Position the reticle
				Transform reticleTransform = homingReticle.transform;
				reticleTransform.position = transform.position;
				reticleTransform.rotation = Quaternion.identity;
				reticleTransform.parent = transform;
			}
		}
	}
	
	public void RemoveHomingBullet(BulletControllerHoming bullet)
	{
		if(bullet != null)
		{
			allHomingBullets.Remove(bullet);
			if((allHomingBullets.Count <= 0) && (homingReticle != null))
			{
				homingReticle.SetActive(false);
				homingReticle = null;
			}
		}
	}

	protected void CloneDetonator()
	{
		GameObject clone = (GameObject)Instantiate(explodeParticle.gameObject, transform.position, Quaternion.identity);
		if(mainRenderer != null)
		{
			Detonator clonedScript = clone.GetComponent<Detonator>();
			clonedScript.color = mainRenderer.color;
		}
	}
}

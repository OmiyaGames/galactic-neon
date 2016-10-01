using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class IEnemy : IDestructable
{
	public const float saturation = 0.7f;
	public const float brightness = 1f;

	public int damage = 1;
	public int maxLife = 1;
	public int score = 1;
	public bool providesPowerUp = true;
	public Detonator explodeParticle = null;

	protected SpriteRenderer spriteRenderer;
	protected int currentLife = 0;
	private readonly HashSet<BulletControllerHoming> allHomingBullets = new HashSet<BulletControllerHoming>();
	private GameObject homingReticle = null;

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

	public void AddHomingBullet(BulletControllerHoming bullet)
	{
		if(bullet != null)
		{
			allHomingBullets.Add(bullet);
			if((homingReticle == null) && (bullet.reticle != null))
			{
				homingReticle = (GameObject)Instantiate(bullet.reticle, transform.position, Quaternion.identity);
				homingReticle.transform.parent = transform;
			}
			else if((homingReticle != null) && (homingReticle.activeSelf == false))
			{
				homingReticle.SetActive(true);
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
			}
		}
	}

	public override void Destroy(MonoBehaviour controller)
	{
		if(explodeParticle != null)
		{
			if((controller is BulletController) || (controller is IPowerUp))
			{
				if(providesPowerUp == true)
				{
					IDestructable powerUp = ObjectSpawner.Instance.CurrentState.RandomPowerUp;
					if(powerUp != null)
					{
						Instantiate(powerUp.gameObject, transform.position, Quaternion.identity);
					}
				}
				CloneDetonator();
			}
			else if(controller is ShipController)
			{
				CloneDetonator();
			}
		}
		Destroy(gameObject);
	}

	public override void Preload(bool state)
	{
		if(state == false)
		{
			CloneDetonator();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	protected void CloneDetonator()
	{
		GameObject clone = (GameObject)Instantiate(explodeParticle.gameObject, transform.position, Quaternion.identity);
		if(spriteRenderer != null)
		{
			Detonator clonedScript = clone.GetComponent<Detonator>();
			clonedScript.color = spriteRenderer.color;
		}
	}
	
	public virtual int Damage(ShipController controller)
	{
		return damage;
	}
	
	public virtual void Hit(BulletController controller)
	{
		Life -= controller.damage;
		if(Life == 0)
		{
			Destroy(controller);
		}
	}
}

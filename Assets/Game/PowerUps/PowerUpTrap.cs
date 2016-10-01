using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpTrap : IPowerUp
{
	public BulletControllerHoming bullet;
	public int maxNumBullets = 20;

	private IDestructable clonedScript = null;

	protected override string Trigger(ShipController instance)
	{
		// Setup variables
		int index = 0;
		GameObject clone = null;
		float diffAngle = 360f / maxNumBullets;
		Vector3 spawnAngle = transform.rotation.eulerAngles;
		BulletControllerHoming[] allGeneratedBullets = new BulletControllerHoming[maxNumBullets];
		int numEnemiesTargeted = 0;

		// Generate all bullets
		for(index = 0; index < maxNumBullets; ++index)
		{
			// Clone bullet
			clone = (GameObject)Instantiate(bullet.gameObject, transform.position, Quaternion.Euler(spawnAngle));
			allGeneratedBullets[index] = clone.GetComponent<BulletControllerHoming>();

			// Update angle
			spawnAngle.z += diffAngle;
			while(spawnAngle.z < 0)
			{
				spawnAngle.z += 360;
			}
		}

		// Search for enemies in range
		foreach(IDestructable candidate in ShipController.Instance.AllDestructables)
		{
			if(candidate is IEnemy) 
			{
				// Target this enemy
				allGeneratedBullets[numEnemiesTargeted].Target = (IEnemy)candidate;
				++numEnemiesTargeted;

				// Make sure the number of targeted enemies doesn't exceed the number of bullets
				if(numEnemiesTargeted >= maxNumBullets)
				{
					// If so, stop searching for enemies
					break;
				}
			}
		}

		// Check if we have any bullets left that hasn't targeted an enemy
		if((numEnemiesTargeted > 0) && (numEnemiesTargeted < maxNumBullets))
		{
			// Change the other bullet's target to the same enemies found earlier
			for(index = numEnemiesTargeted; index < maxNumBullets; ++index)
			{
				allGeneratedBullets[index].Target = allGeneratedBullets[(index % numEnemiesTargeted)].Target;
			}
		}

		return "Homing Bomb";
	}
	
	public override void Preload(bool state)
	{
		if(state == false)
		{
			GameObject clone = (GameObject)Instantiate(bullet.gameObject, transform.position, Quaternion.identity);
			clonedScript = clone.GetComponent<IDestructable>();
		}
		else
		{
			Destroy(gameObject);
		}
		
		if(clonedScript != null)
		{
			clonedScript.Preload(state);
		}
	}
}

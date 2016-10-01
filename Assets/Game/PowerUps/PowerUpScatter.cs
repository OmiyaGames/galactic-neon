using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpScatter : IPowerUp
{
	public IDestructable bullet;
	public int numBullets = 12;

	private IDestructable clonedScript = null;

	protected override string Trigger(ShipController instance)
	{
		// Calculate the other 6 angles
		float diffAngle = 360f / numBullets;
		Vector3 spawnAngle = transform.rotation.eulerAngles;
		for(int index = 0; index < numBullets; ++index)
		{
			Instantiate(bullet.gameObject, transform.position, Quaternion.Euler(spawnAngle));
			spawnAngle.z += diffAngle;
			while(spawnAngle.z < 0)
			{
				spawnAngle.z += 360;
			}
		}
		return "Scatter Bomb";
	}

	public override void Preload(bool state)
	{
		if(state == false)
		{
			GameObject clone = (GameObject)Instantiate(bullet.gameObject, transform.position, Quaternion.identity);
			clonedScript = clone.GetComponent<IDestructable>();
		}
		else if(gameObject != null)
		{
			Destroy(gameObject);
		}

		if(clonedScript != null)
		{
			clonedScript.Preload(state);
		}
	}
}

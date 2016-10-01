using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpScatter : IPowerUp
{
	public IDestructable bullet;
	public int numBullets = 12;

	protected override string Trigger(ShipController instance)
	{
		// Calculate the other 6 angles
		float diffAngle = 360f / numBullets;
		Vector3 spawnAngle = transform.rotation.eulerAngles;
		for(int index = 0; index < numBullets; ++index)
		{
			GlobalGameObject.Get<PoolingManager>().GetInstance(bullet.gameObject, transform.position, Quaternion.Euler(spawnAngle));
			spawnAngle.z += diffAngle;
			while(spawnAngle.z < 0)
			{
				spawnAngle.z += 360;
			}
		}
		return "Scatter Bomb";
	}
}

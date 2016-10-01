using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletControllerExplode : BulletController
{
	public IDestructable bullet;
	public int numBullets = 6;

	protected override void OnTriggerEnter2D(Collider2D info)
	{
		if(info.CompareTag("Enemy") == true)
		{
			// Kill the enemy
			IEnemy enemy = info.gameObject.GetComponent<IEnemy>();
			if(enemy.Score > 0)
			{
				GameObject clone = GlobalGameObject.Get<PoolingManager>().GetInstance(scoreLabel.gameObject, enemy.transform.position, Quaternion.identity);
				ShipController.Instance.IncrementScore(clone.GetComponent<ScoreLabel>(), enemy.Score);
			}
			enemy.Hit(this);
			Destroy(enemy);

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
		}
	}
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletControllerBomb : BulletController
{
	public Detonator explodeParticle = null;
	public float killRadius = 6;

	protected override void OnTriggerEnter2D(Collider2D info)
	{
		if(info.CompareTag("Enemy") == true)
		{
			// Kill the enemy
			ExplodeBomb();
		}
	}

	void ExplodeBomb()
	{
		// Kill nearby enemies
		IEnemy enemy = null;
		killRadius *= killRadius;
		foreach(IDestructable candidate in ShipController.Instance.AllDestructables)
		{
			if((candidate != null) && (candidate is IEnemy) && (ShipController.DistanceSquared(transform.position, candidate.transform.position) < killRadius))
			{
				enemy = (IEnemy)candidate;
				if(enemy.Score > 0)
				{
					GameObject clone = (GameObject)Instantiate(scoreLabel.gameObject, enemy.transform.position, Quaternion.identity);
					ShipController.Instance.IncrementScore(clone.GetComponent<ScoreLabel>(), enemy.Score);
				}
				enemy.Hit(this);
			}
		}

		// Explode
		if(explodeParticle != null)
		{
			Instantiate(explodeParticle.gameObject, transform.position, Quaternion.identity);
		}
		Destroy(enemy);
	}
}

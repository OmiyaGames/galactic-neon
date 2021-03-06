﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpBomb : IPowerUp
{
	public Detonator explodeParticle = null;
	public float killRadius = 6;
	public ScoreLabel scoreLabel = null;

	protected override string Trigger(ShipController instance)
	{
		// Kill nearby enemies
		IEnemy enemy = null;
		killRadius *= killRadius;
		foreach(IDestructable candidate in IGameManager.Instance.AllDestructables())
		{
			if((candidate is IEnemy) && (IGameManager.DistanceSquared(transform.position, candidate.transform.position) < killRadius))
			{
				enemy = (IEnemy)candidate;
				if(enemy.Score > 0)
				{
					GameObject clone = GlobalGameObject.Get<PoolingManager>().GetInstance(scoreLabel.gameObject, enemy.transform.position, Quaternion.identity);
					ShipController.Instance.IncrementScore(clone.GetComponent<ScoreLabel>(), enemy.Score);
				}
				enemy.Destroy(this);
			}
		}
		
		// Explode
		if(explodeParticle != null)
		{
			Instantiate(explodeParticle.gameObject, transform.position, Quaternion.identity);
		}
		return "Explosive Bomb";
	}
}

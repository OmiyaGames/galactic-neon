using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletControllerStrong : BulletController
{
	protected override void OnTriggerEnter2D(Collider2D info)
	{
		if(info.CompareTag("Enemy") == true)
		{
			IEnemy enemy = info.gameObject.GetComponent<IEnemy>();
			if(enemy.Score > 0)
			{
				GameObject clone = (GameObject)Instantiate(scoreLabel.gameObject, enemy.transform.position, Quaternion.identity);
				ShipController.Instance.IncrementScore(clone.GetComponent<ScoreLabel>(), enemy.Score);
			}
			enemy.Hit(this);
		}
	}
}

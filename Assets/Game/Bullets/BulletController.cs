using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class BulletController : IDestructable
{
	public int damage = 1;
	public float velocity = 100f;
	public ScoreLabel scoreLabel = null;
	public Vector2 pitchRange = new Vector2(0.5f, 1.5f);

	protected Vector3 velocityVector;

	public override void Destroy(MonoBehaviour controller)
	{
		Destroy(gameObject);
	}

	public override void Preload(bool state)
	{
		if(state == true)
		{
			Destroy(gameObject);
		}
	}

	protected virtual void Start()
	{
		GetComponent<Collider2D>().isTrigger = true;
		if(ShipController.Instance != null)
		{
			ShipController.Instance.AddDestructable(this);
			velocityVector = (transform.up * velocity);
			velocityVector.x += ShipController.Instance.GetComponent<Rigidbody2D>().velocity.x;
			velocityVector.y += ShipController.Instance.GetComponent<Rigidbody2D>().velocity.y;
			GetComponent<AudioSource>().pitch = Random.Range(pitchRange.x, pitchRange.y);
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().Play();
		}
	}

	protected virtual void FixedUpdate ()
	{
		GetComponent<Rigidbody2D>().transform.position += (velocityVector * Time.deltaTime);
	}

	protected virtual void OnTriggerEnter2D(Collider2D info)
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
			Destroy(this);
		}
	}
}

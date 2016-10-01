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

    private GameObject objectCache = null;

    public GameObject BulletObject
    {
        get
        {
            if(objectCache == null)
            {
                objectCache = gameObject;
            }
            return objectCache;
        }
    }

	public override void Destroy(MonoBehaviour controller)
	{
        BulletObject.SetActive(false);
	}

	public override void Start()
	{
		base.Start();

		collider2D.isTrigger = true;
		if((ShipController.Instance != null) && (IGameManager.Instance != null))
		{
			// Add this element into the game manager
			IGameManager.Instance.AddDestructable(this);

			velocityVector = (transform.up * velocity);
			velocityVector.x += ShipController.Instance.rigidbody2D.velocity.x;
			velocityVector.y += ShipController.Instance.rigidbody2D.velocity.y;
			audio.pitch = Random.Range(pitchRange.x, pitchRange.y);
			audio.Stop();
			audio.Play();
		}
	}

	protected virtual void FixedUpdate ()
	{
		rigidbody2D.transform.position += (velocityVector * Time.deltaTime);
	}

	protected virtual void OnTriggerEnter2D(Collider2D info)
	{
		if(info.CompareTag("Enemy") == true)
		{
			IEnemy enemy = info.gameObject.GetComponent<IEnemy>();
			if(enemy.Score > 0)
			{
				GameObject clone = GlobalGameObject.Get<PoolingManager>().GetInstance(scoreLabel.gameObject, enemy.transform.position, Quaternion.identity);
				ShipController.Instance.IncrementScore(clone.GetComponent<ScoreLabel>(), enemy.Score);
			}
			enemy.Hit(this);
			Destroy(this);
		}
	}
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class EnemyKillerTrigger : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
	    collider2D.isTrigger = true;
	}
	
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy") == true)
        {
            IEnemy enemyScript = other.GetComponent<IEnemy>();
            enemyScript.Destroy(this);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class HomingTrigger : MonoBehaviour
{
	private const float DistanceThresholdSqr = 200 * 200;

	private readonly List<IEnemy> enemiesInRange = new List<IEnemy>();
	private IEnemy closestEnemy = null;
	private int index = 0;
	private float closestDistanceSqr = DistanceThresholdSqr + 1;
	private Vector2 distance = Vector2.zero;
	private Vector3 localPosition = Vector3.zero;
	private Quaternion localRotation = Quaternion.identity;

	public IEnemy ClosestEnemy
	{
		get
		{
			return closestEnemy;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.CompareTag("Enemy") == true)
		{
			IEnemy script = other.GetComponent<IEnemy>();
			if(script != null)
			{
				enemiesInRange.Add(script);
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if(other.CompareTag("Enemy") == true)
		{
			IEnemy script = other.GetComponent<IEnemy>();
			if(script != null)
			{
				enemiesInRange.Remove(script);
			}
		}
	}

	void FixedUpdate()
	{
		// Reset values
		closestEnemy = null;
		closestDistanceSqr = DistanceThresholdSqr + 1;
		transform.localPosition = localPosition;
		transform.localRotation = localRotation;

		// Go through every enemy
		for(index = 0; index < enemiesInRange.Count; ++index)
		{
			// Check if enemy is not null
			if(enemiesInRange[index] == null)
			{
				// If so, remove it from the list
				enemiesInRange.RemoveAt(index);
				--index;
			}
			else if(closestDistanceSqr > DistanceThresholdSqr)
			{
				// If this is the first enemy listed, target this enemy
				closestEnemy = enemiesInRange[index];

				// Calculate the distance
				distance.x = closestEnemy.transform.position.x - transform.position.x;
				distance.y = closestEnemy.transform.position.y - transform.position.y;
				closestDistanceSqr = distance.sqrMagnitude;
			}
			else
			{
				// Calculate the distance
				distance.x = closestEnemy.transform.position.x - transform.position.x;
				distance.y = closestEnemy.transform.position.y - transform.position.y;

				// Check if this is the closest enemy
				if(distance.sqrMagnitude < closestDistanceSqr)
				{
					// Update the closest enemy
					closestEnemy = enemiesInRange[index];
					closestDistanceSqr = distance.sqrMagnitude;
				}
			}
		}
	}

	public void Reset()
	{
		enemiesInRange.Clear();
	}
}

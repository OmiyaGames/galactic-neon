using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundGenerator : MonoBehaviour
{
	public BackgroundSprite backgroundPrefab = null;
	public int numPrefabsPerIteration = 10;
	public float gapPerIteration = 0.2f;
	public int numIterations = 6;
	public float maxRange = 20f;
	public float explosionStrength = 1f;
	public float maxExplosionDistance = 1f;
	public float alpha = 0.7f;

	private readonly List<BackgroundSprite> allSprites = new List<BackgroundSprite>();
	private int index = 0;
	private float currentTime = 0;
	private Vector2 explosionPosition;
	private Vector2 explosionDirection;

	public void ApplyExplosionForce(Vector3 position)
	{
		float maxDistanceSqr = maxExplosionDistance * maxExplosionDistance;
		float distanceSqr = 0f;

		// Convert position to Vector2
		explosionPosition.x = position.x;
		explosionPosition.y = position.y;
		for(index = 0; index < allSprites.Count; ++index)
		{
			// Calculate direction
			explosionDirection.x = allSprites[index].transform.position.x - explosionPosition.x;
			explosionDirection.y = allSprites[index].transform.position.y - explosionPosition.y;

			// Calculate distance
			distanceSqr = explosionDirection.sqrMagnitude;
			if(distanceSqr < maxDistanceSqr)
			{
				explosionDirection.Normalize();
				explosionDirection *= (explosionStrength * ((maxDistanceSqr - distanceSqr) / maxDistanceSqr));
				allSprites[index].rigidbody2D.AddForceAtPosition(explosionDirection, explosionPosition);
			}
		}
	}

	// Use this for initialization
	IEnumerator Start ()
	{
		GameObject clone = null;
		BackgroundSprite clonedScript = null;
		int numPrefab = 0, iteration = 0;
		for(; iteration < numIterations; ++iteration)
		{
			for(numPrefab = 0; numPrefab < numPrefabsPerIteration; ++numPrefab)
			{
				clone = (GameObject)Instantiate(backgroundPrefab.gameObject, transform.position, Quaternion.identity);
				clone.transform.parent = transform;
				clonedScript = clone.GetComponent<BackgroundSprite>();
				clonedScript.MaxDistanceFromShip = maxRange;
				clonedScript.Alpha = alpha;
				allSprites.Add(clonedScript);
			}
			yield return new WaitForSeconds(gapPerIteration);
		}
	}

	void Update()
	{
		currentTime = Time.time;
		for(index = 0; index < allSprites.Count; ++index)
		{
			if((allSprites[index].StartTime > 0) && ((currentTime - allSprites[index].StartTime) > allSprites[index].duration))
			{
				allSprites[index].Start();
			}
			else
			{
				allSprites[index].spriteRenderer.transform.Rotate(0, 0, (allSprites[index].RotateSpeed * Time.deltaTime));
			}
		}
	}
}

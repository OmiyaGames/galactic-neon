using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundGenerator : IBackgroundGenerator
{
	public BackgroundSprite backgroundPrefab = null;
	public int numPrefabsPerIteration = 10;
	public float gapPerIteration = 0.2f;
	public int numIterations = 6;
	public float maxRange = 20f;
	public float explosionStrength = 1f;
	public float maxExplosionDistance = 1f;
    public Vector2 zRange = new Vector2(10f, 40f);
    public Vector2 alphaRange = new Vector2(0.2f, 0.8f);

    GameObject backgroundPrefabObject = null;
	readonly List<BackgroundSprite> allSprites = new List<BackgroundSprite>();
	Vector2 explosionPosition;
	Vector2 explosionDirection;
    float zRangeGap = -1f;
    float alphaRangeGap = -1f;

    public override void ApplyExplosionForce(Vector3 position)
	{
		float maxDistanceSqr = maxExplosionDistance * maxExplosionDistance;
		float distanceSqr = 0f;

		// Convert position to Vector2
		explosionPosition.x = position.x;
		explosionPosition.y = position.y;
        for(int index = 0; index < allSprites.Count; ++index)
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

    protected override void Activate()
    {
        StartCoroutine(GenerateSprites());
    }

    protected override void Deactivate()
    {
        for(int index = 0; index < allSprites.Count; ++index)
        {
            allSprites[index].Deactivate();
        }
        allSprites.Clear();
    }

    // Use this for initialization
	IEnumerator GenerateSprites()
	{
        allSprites.Clear();
        if(backgroundPrefabObject == null)
        {
            backgroundPrefabObject = backgroundPrefab.gameObject;
        }

        // Setup variables
		GameObject clone = null;
		BackgroundSprite clonedScript = null;
		int numPrefab = 0, iteration = 0;
        Vector3 position = transform.position;

        // Generate background sprites
		for(; iteration < numIterations; ++iteration)
		{
			for(numPrefab = 0; numPrefab < numPrefabsPerIteration; ++numPrefab)
			{
                // Randomize z-axis
                position.z = Random.Range(zRange.x, zRange.y);

                // Clone the sprite
				clone = GlobalGameObject.Get<PoolingManager>().GetInstance(
                    backgroundPrefabObject, position, Quaternion.identity);
				clonedScript = clone.GetComponent<BackgroundSprite>();
				clonedScript.MaxDistanceFromShip = maxRange;
                clonedScript.Alpha = ZToAlpha(position.z);
				allSprites.Add(clonedScript);
			}
			yield return new WaitForSeconds(gapPerIteration);
		}
	}

    float ZToAlpha(float z)
    {
        float returnAlpha = 0;

        // Get the difference between max and min value
        if(zRangeGap < 0)
        {
            zRangeGap = zRange.y - zRange.x;
        }
        if(alphaRangeGap < 0)
        {
            alphaRangeGap = alphaRange.y - alphaRange.x;
        }

        // Get normalized value of zRange
        returnAlpha = ((z - zRange.x) / zRangeGap);

        // Convert this value to alpha
        returnAlpha = (returnAlpha * alphaRangeGap) + alphaRange.x;
        return returnAlpha;
    }
}

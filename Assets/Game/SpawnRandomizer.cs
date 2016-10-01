using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnRandomizer : MonoBehaviour
{
	public int index = 0;
	public int scoreThreshold = 100;
	public float waitBeforeSpawning = 1f;
	public int numberOfObjectsToSpawnAtOnce = 1;
	[System.Serializable]
	public class Spawn
	{
		public IDestructable spawnObject = null;
		public int spawnLikeliness = 1;
	}
	public Spawn[] spawnedObject;
	public Spawn[] powerUps;

	private List<IDestructable> objectRandomizer = new List<IDestructable>();
	private List<IDestructable> powerUpRandomizer = new List<IDestructable>();

	public IDestructable RandomObject
	{
		get
		{
			IDestructable returnValue = null;
			if(objectRandomizer.Count > 0)
			{
				returnValue = objectRandomizer[Random.Range(0, objectRandomizer.Count)];
			}
			return returnValue;
		}
	}

	public IDestructable RandomPowerUp
	{
		get
		{
			IDestructable returnValue = null;
			if(powerUpRandomizer.Count > 0)
			{
				returnValue = powerUpRandomizer[Random.Range(0, powerUpRandomizer.Count)];
			}
			return returnValue;
		}
	}

	void Start()
	{
		if(spawnedObject != null)
		{
			int instances = 0, index = 0;
			for(index = 0; index < spawnedObject.Length; ++index)
			{
				for(instances = 0; instances < spawnedObject[index].spawnLikeliness; ++instances)
				{
					objectRandomizer.Add(spawnedObject[index].spawnObject);
				}
			}
			for(index = 0; index < powerUps.Length; ++index)
			{
				for(instances = 0; instances < powerUps[index].spawnLikeliness; ++instances)
				{
					powerUpRandomizer.Add(powerUps[index].spawnObject);
				}
			}
		}
	}
}
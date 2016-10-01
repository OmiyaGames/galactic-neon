using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
	private static ObjectSpawner msInstance = null;
	public float distanceFromCamera = 10f;
	public int maxNumObjects = 45;
	public Camera mainCamera;

	public float hueChangeSpeed = 0.1f;
	public float saturation = 1f;
	public float brightness = 0.1f;

	public Transform preloadLocation;

	//public float preloadDelay = 0.1f;

	private float hue = 0;
	private Color backgroundColor = Color.red;

	private Vector3 mCameraCenterPosition;
	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;
	private float mLastSpawned = 0f;
	private int mIndex = 0;
	private int mSpawnStateIndex = 0;
	private int mCumulativeScoreThreshold = 0;
	private IDestructable mClone;
	private readonly SortedDictionary<int, List<SpawnRandomizer>> mAllSpawnStates = new SortedDictionary<int, List<SpawnRandomizer>>();
	private readonly List<SpawnRandomizer> mCurrentSpawnStates = new List<SpawnRandomizer>();

	public static ObjectSpawner Instance
	{
		get
		{
			return msInstance;
		}
	}

	public SpawnRandomizer CurrentState
	{
		get
		{
			return mCurrentSpawnStates[mSpawnStateIndex];
		}
	}

	void Awake()
	{
		msInstance = this;
	}

	void Start()
	{
		// Bind to the start event
		ShipController.Instance.startEvent += Reset;
		distanceFromCamera *= ShipController.ScreenRatioMultiplier;

		// Search for all the spawn randomizer
		List<SpawnRandomizer> spawnStateList = null;
		SpawnRandomizer[] allRandomizers = GetComponentsInChildren<SpawnRandomizer>();
		for(int index = 0; index < allRandomizers.Length; ++index)
		{
			if(allRandomizers[index] != null)
			{
				// Check if the randomizer's index is already added into the dictionary
				if(mAllSpawnStates.TryGetValue(allRandomizers[index].index, out spawnStateList) == true)
				{
					// Add it to this index's list
					spawnStateList.Add(allRandomizers[index]);
				}
				else
				{
					// Create a new list of spawn states
					spawnStateList = new List<SpawnRandomizer>(3);
					spawnStateList.Add(allRandomizers[index]);

					// Add it to the dictionary
					mAllSpawnStates.Add(allRandomizers[index].index, spawnStateList);
				}
			}
		}

		// Update hue
		hue = Random.value;
		IPowerUp.FromHSV(hue, saturation, brightness, ref backgroundColor);
		mainCamera.backgroundColor = backgroundColor;

		// Preload everything
		StartCoroutine(PreloadEverything());
	}

	void OnDestroy()
	{
		msInstance = null;
	}

	// Update is called once per frame
	void Update ()
	{
		if((ShipController.Instance.ShipState == ShipController.State.Playing) && (ShipController.Instance.NumDestructables < maxNumObjects) && ((Time.time - mLastSpawned) > CurrentState.waitBeforeSpawning))
		{
			// Check if the ship exceeded the current threshold
			if((mSpawnStateIndex < (mCurrentSpawnStates.Count - 1)) && (ShipController.Instance.Score >= mCumulativeScoreThreshold))
			{
				++mSpawnStateIndex;
				mCumulativeScoreThreshold += CurrentState.scoreThreshold;
			}

			// Get the center of where we want to spawn the object
			mCameraCenterPosition = Camera.main.transform.position;
			mCameraCenterPosition.z = ShipController.Instance.transform.position.z;

			for(mIndex = 0; mIndex < CurrentState.numberOfObjectsToSpawnAtOnce; ++mIndex)
			{
				// Randomize spawn location
				mSpawnPosition = mCameraCenterPosition;
				mRandomOffset = Random.insideUnitCircle.normalized;
				mSpawnPosition.x += (mRandomOffset.x * distanceFromCamera);
				mSpawnPosition.y += (mRandomOffset.y * distanceFromCamera);

				// Spawn the object there
				mClone = CurrentState.RandomObject;
				if(mClone != null)
				{
					Instantiate(mClone.gameObject, mSpawnPosition, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
				}
			}

			// Update the last spawn time
			mLastSpawned = Time.time;
		}

		// Change background color
		hue += hueChangeSpeed * Time.deltaTime;
		if(hue > 1)
		{
			hue = 0;
		}
		IPowerUp.FromHSV(hue, saturation, brightness, ref backgroundColor);
		mainCamera.backgroundColor = backgroundColor;
	}

	void Reset(ShipController instance)
	{
		// Reset values
		mIndex = 0;
		mLastSpawned = 0;
		mSpawnStateIndex = 0;

		// Reset list of spawns
		mCurrentSpawnStates.Clear();
		foreach(KeyValuePair<int, List<SpawnRandomizer>> states in mAllSpawnStates)
		{
			if((states.Value != null) && (states.Value.Count > 0))
			{
				if(states.Value.Count == 1)
				{
					mCurrentSpawnStates.Add(states.Value[0]);
				}
				else
				{
					mCurrentSpawnStates.Add(states.Value[Random.Range(0, states.Value.Count)]);
				}
			}
		}
		mCumulativeScoreThreshold = mCurrentSpawnStates[mSpawnStateIndex].scoreThreshold;
	}

	IEnumerator PreloadEverything()
	{
		int index = 0, innerIndex = 0;
		HashSet<IDestructable> coveredObjects = new HashSet<IDestructable>();
		SpawnRandomizer.Spawn[] spawnedObjects = null;
		GameObject clone = null;
		IDestructable clonedScript;

		foreach(KeyValuePair<int, List<SpawnRandomizer>> states in mAllSpawnStates)
		{
			if(states.Value != null)
			{
				for(index = 0; index < states.Value.Count; ++index)
				{
					spawnedObjects = states.Value[index].spawnedObject;
					for(innerIndex = 0; innerIndex < spawnedObjects.Length; ++innerIndex)
					{
						if((spawnedObjects[innerIndex].spawnObject != null) && (coveredObjects.Contains(spawnedObjects[innerIndex].spawnObject) == false))
						{
							coveredObjects.Add(spawnedObjects[innerIndex].spawnObject);
							clone = (GameObject)Instantiate(spawnedObjects[innerIndex].spawnObject.gameObject, preloadLocation.position, Quaternion.identity);
							clonedScript = clone.GetComponent<IDestructable>();
							clonedScript.Preload(false);
							yield return null;
							clonedScript.Preload(true);
						}
					}

					spawnedObjects = states.Value[index].powerUps;
					for(innerIndex = 0; innerIndex < spawnedObjects.Length; ++innerIndex)
					{
						if((spawnedObjects[innerIndex].spawnObject != null) && (coveredObjects.Contains(spawnedObjects[innerIndex].spawnObject) == false))
						{
							coveredObjects.Add(spawnedObjects[innerIndex].spawnObject);
							clone = (GameObject)Instantiate(spawnedObjects[innerIndex].spawnObject.gameObject, preloadLocation.position, Quaternion.identity);
							clonedScript = clone.GetComponent<IDestructable>();
							clonedScript.Preload(false);
							yield return null;
							clonedScript.Preload(true);
						}
					}
				}
			}
		}
	}
}

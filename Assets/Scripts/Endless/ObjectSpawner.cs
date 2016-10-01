using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : IGameManager
{
	public float distanceFromCamera = 10f;
	public int maxNumObjects = 45;

	[Range(1, 100)]
	public float destroyRange = 45f;

	private Vector3 mCameraCenterPosition;
	private Vector3 mSpawnPosition;
	private Vector2 mRandomOffset;
	private float mLastSpawned = 0f;
	private int mIndex = 0;
	private int mSpawnStateIndex = 0;
	private int mCumulativeScoreThreshold = 0;
	private IDestructable mClone;

	private readonly List<IDestructable> destructables = new List<IDestructable>();
	private readonly SortedDictionary<int, List<SpawnRandomizer>> mAllSpawnStates = new SortedDictionary<int, List<SpawnRandomizer>>();
	private readonly List<SpawnRandomizer> mCurrentSpawnStates = new List<SpawnRandomizer>();

	#region Overrides
	
	public override int NumDestructables
	{
		get
		{
		    return destructables.Count;
		}
	}
	
	public override IEnumerable<IDestructable> AllDestructables()
	{
        for(int index = 0; index < destructables.Count; ++index)
        {
            yield return destructables[index];
        }
	}
	
	public override void AddEnemy(IEnemy enemy)
	{
		if(enemy != null)
		{
		    destructables.Add(enemy);
		}
	}
	
	public override void AddDestructable(IDestructable destructable)
	{
		if(destructable != null)
		{
		    destructables.Add(destructable);
    	}
	}

	#endregion

	public SpawnRandomizer CurrentState
	{
		get
		{
			return mCurrentSpawnStates[mSpawnStateIndex];
		}
	}

	void Start()
	{
		// Setup destroy range
		destroyRange *= IGameManager.ScreenRatioMultiplier;
		destroyRange *= destroyRange;

		// Bind to the start event
		ShipController.Instance.startEvent += Reset;
		ShipController.Instance.deadEvent += DestroyAllDestructables;
		distanceFromCamera *= IGameManager.ScreenRatioMultiplier;

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
	}

	// Update is called once per frame
	void Update ()
	{
		if(ShipController.Instance.ShipState == ShipController.State.Playing)
		{
			// Check if we can still spawn objects
			if((NumDestructables < maxNumObjects) && ((Time.time - mLastSpawned) > CurrentState.waitBeforeSpawning))
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

				GameObject cloneObject = null;
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
						cloneObject = GlobalGameObject.Get<PoolingManager>().GetInstance(mClone.gameObject, mSpawnPosition, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
						AddDestructable(cloneObject.GetComponent<IDestructable>());
					}
				}

				// Update the last spawn time
				mLastSpawned = Time.time;
			}

			// Destroy far away objects
			DestroyFarAwayDestructables();
		}
	}

	void DestroyFarAwayDestructables ()
	{
		IDestructable toRemove = null;
        for(int index = 0; index < destructables.Count; ++index)
		{
			toRemove = destructables[index];
			if (toRemove.gameObject.activeSelf == false)
			{
				destructables.RemoveAt(index);
				--index;
			}
			else if(IGameManager.DistanceSquared(toRemove.transform.position, transform.position) > destroyRange)
			{
				toRemove.Destroy(null);
				destructables.RemoveAt(index);
				--index;
			}
		}
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

	void DestroyAllDestructables(ShipController instance)
	{
        // Destroy everything
        for(int index = 0; index < destructables.Count; ++index)
        {
            if(destructables[index] != null)
            {
                destructables[index].Destroy(this);
            }
        }
		destructables.Clear ();
	}
}

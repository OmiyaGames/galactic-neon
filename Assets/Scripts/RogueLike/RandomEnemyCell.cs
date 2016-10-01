using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomEnemyCell : IGridCell
{
    [Header("Enemy")]
	[Tooltip("The length of time, in seconds, this cell persists when deactivated before it's marked as expired. Set to a negative value if the cell shouldn't expire.")]
	public float expireAfter = 20f;
	// FIXME: testing variable.  We probably need to be able to customize this more.
	public IEnemy[] spawnObjects;
	// FIXME: testing variable.  We probably need to be able to customize this more.
	public int numObjectsToSpawn;

	private IGridCell.State currentState = IGridCell.State.InactiveNew;
	private float timeLastDeactivated = 0f;

	public override bool IsAvailable
	{
		get
		{
            // FIXME: check the level of the ship, and update the return value
			return true;
		}
	}

    public override IGridCell.State CurrentState
	{
		get
		{
			// Check if this cell is in the inactive, persistent state, and can expire 
			if((currentState == IGridCell.State.InactivePersist) && (expireAfter > 0))
			{
				// If so, check if the cell has already expired
				if((Time.time - timeLastDeactivated) > expireAfter)
				{
					// Mark the cell as expired
					currentState = IGridCell.State.InactiveExpired;
				}
			}
			return currentState;
		}
	}

	public override void Initialized(PoolingManager manager)
	{
		base.Initialized(manager);
		currentState = IGridCell.State.InactiveNew;
	}

	public override void Activated(PoolingManager manager)
	{
		base.Activated(manager);
		currentState = IGridCell.State.InactiveNew;
	}

	public override void Activate()
	{
		// Check if this cell is new
		if(currentState == IGridCell.State.InactiveNew)
		{
			// FIXME: for testing purposes, spawning objects at a random location
			IEnemy objectToSpawn = null;
			IEnemyInfo newInfo = null;
			for(int index = 0; index < numObjectsToSpawn; ++index)
			{
				// Get a random object
				objectToSpawn = null;
				if(spawnObjects.Length > 0)
				{
					objectToSpawn = spawnObjects[Random.Range(0, spawnObjects.Length)];
				}
				
				// Check if there's an object to spawn
				if(objectToSpawn != null)
				{
					// Grab a random location
					Vector3 cloneLocation = Position;
					cloneLocation.x = Random.Range(CellRange.xMin, CellRange.xMax);
					cloneLocation.y = Random.Range(CellRange.yMin, CellRange.yMax);
					
					// Grab a random rotation
					Quaternion cloneRotation = Quaternion.Euler(0, 0, (Random.value * 360f));

					// Generate a new enemy information
					newInfo = objectToSpawn.GenerateNewInfo(cloneLocation, cloneRotation);
					if(newInfo != null)
					{
						// Add this enemy information to the information set
						EnemyInformationSet.Add(newInfo);
					}
				}
			}
		}

		// Update the state
		currentState = IGridCell.State.Active;
	}
	
	public override void Deactivate()
	{
		// Make sure the cell isn't deactivated already
		if(currentState == IGridCell.State.Active)
		{
			timeLastDeactivated = Time.time;
		}

        // Update the state
        currentState = IGridCell.State.InactivePersist;
    }
}

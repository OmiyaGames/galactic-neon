using UnityEngine;
using System.Collections;

public class TestCell : IGridCell
{
    [Header("Test")]
    public IEnemy spawnObject;
    public IGridCell.State deactivateState = IGridCell.State.InactiveExpired;

    private IGridCell.State currentState = IGridCell.State.InactiveNew;

    public override bool IsAvailable
    {
        get
        {
            return true;
        }
    }
    
    public override IGridCell.State CurrentState
    {
        get
        {
            return currentState;
        }
    }
    
    public override void Initialized(PoolingManager manager)
    {
        base.Initialized(manager);
        currentState = IGridCell.State.InactiveNew;
        //Debug.Log("Initialized() called");
    }
    
    public override void Activated(PoolingManager manager)
    {
        base.Activated(manager);
        currentState = IGridCell.State.InactiveNew;
        //Debug.Log("Activated() called");
    }
    
    public override void Activate()
    {
        //Debug.Log(string.Format("Activating Cell ({0} x {1}) from State {2}", X, Y, currentState));

        // Check if this cell is new
        if(currentState == IGridCell.State.InactiveNew)
        {
            // Check if there's an object to spawn
            if(spawnObject != null)
            {
                // Generate a new enemy information
                IEnemyInfo newInfo = spawnObject.GenerateNewInfo(Position, Quaternion.identity);
                if(newInfo != null)
                {
                    // Add this enemy information to the information set
                    EnemyInformationSet.Add(newInfo);
                }
            }
        }
        
        // Update the state
        CellObject.SetActive(true);
        currentState = IGridCell.State.Active;
    }
    
    public override void Deactivate()
    {
        //Debug.Log(string.Format("Deactivating Cell ({0} x {1}) from State {2}", X, Y, currentState));

        // Update the state
        currentState = deactivateState;
    }
}

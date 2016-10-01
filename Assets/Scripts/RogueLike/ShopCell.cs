using UnityEngine;
using System.Collections;

public class ShopCell : IGridCell
{
    [Header("Enemy Killer")]
    [Range(0, 1)]
    public float killEnemiesIfWithinPercent = 0.75f;
    public Collider2D killEnemiesCollider;

    [Header("Shop")]
    public Transform spawnPoint;

    IGridCell.State currentState = IGridCell.State.InactivePersist;

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
        currentState = IGridCell.State.InactivePersist;
    }
    
    public override void Activated(PoolingManager manager)
    {
        base.Activated(manager);
        currentState = IGridCell.State.InactivePersist;
    }
    
    public override void Activate()
    {
        // Update the state
        currentState = IGridCell.State.Active;
    }

    public override void Deactivate()
    {
        // Update the state
        currentState = IGridCell.State.InactiveExpired;
    }

    protected override bool UpdateCellRange()
    {
        // Make sure the manager and transform are available
        bool returnFlag = base.UpdateCellRange();

        // Update the enemy killer boundaries
        if((returnFlag == true) && (Manager != null) && (Manager.Grid != null) && (CellTransform != null))
        {
            Bounds bounds = killEnemiesCollider.bounds;
            Vector3 extents = bounds.extents;
            extents.x = Manager.Grid.cellSize * killEnemiesIfWithinPercent;
            extents.y = Manager.Grid.cellSize * killEnemiesIfWithinPercent;
            bounds.extents = extents;
        }
        return returnFlag;
    }

    public void EnterShop(Vector3 shopPosition)
    {
        ShipController.Instance.UpdateShop(true);
        ShipController.Position = shopPosition;
        //FIXME: bring up the shop GUI
    }

    public void ExitShop()
    {
        ShipController.Instance.UpdateShop(false);
        ShipController.Position = spawnPoint.position;
        //FIXME: bring up the shop GUI
    }
}

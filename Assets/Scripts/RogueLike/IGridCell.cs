using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class IGridCell : IPooledObject
{
    public enum State
    {
        /// <summary>
        /// The cell is currently inactive,
        /// and the ship has yet to visit the cell.
        /// </summary>
        InactiveNew,
        /// <summary>
        /// The cell is currently inactive,
        /// and the ship has recently visited the cell.
        /// </summary>
        InactivePersist,
        /// <summary>
        /// The cell is currently inactive,
        /// and the ship has visited the cell, but not for a while.
        /// </summary>
        InactiveExpired,
        /// <summary>
        /// The cell is currently activated.
        /// This happens if the ship is either in the cell,
        /// or close enough to it.
        /// </summary>
        Active
    }

    [Header("Background")]
    public BackgroundManager.Type backgroundType;

    Transform cellTransform = null;
    GridManager manager = null;
    Rect cellRange = new Rect();
    GameObject objectCache = null;
    bool updateCellRange = true;

    // Stores enemy information
    readonly List<IEnemyInfo> enemyInformationSet = new List<IEnemyInfo>();

    public abstract bool IsAvailable
    {
        get;
    }
    
    public abstract IGridCell.State CurrentState
    {
        get;
    }

    #region Public Properties

    public int TypeIdentifier
    {
        get;
        set;
    }
    
    public string OriginalName
    {
        get;
        set;
    }

    public int X
    {
        get;
        set;
    }

    public int Y
    {
        get;
        set;
    }
    
    public GridManager Manager
    {
        get
        {
            return manager;
        }
        set
        {
            manager = value;
            updateCellRange = true;
        }
    }
    
    public Vector3 Position
    {
        get
        {
            return CellTransform.position;
        }
        set
        {
            CellTransform.position = value;
            updateCellRange = true;
        }
    }
    
    public Rect CellRange
    {
        get
        {
            UpdateCellRange();
            return cellRange;
        }
    }

    public GameObject CellObject
    {
        get
        {
            if(objectCache == null)
            {
                objectCache = gameObject;
            }
            return objectCache;
        }
    }

    public bool IsReady
    {
        get
        {
            return (updateCellRange == false);
        }
    }

    #endregion

    public abstract void Activate();
    public abstract void Deactivate();

    public override void Initialized(PoolingManager pool)
    {
        base.Initialized(pool);
        EnemyInformationSet.Clear();
        updateCellRange = true;
    }
    
    public override void Activated(PoolingManager pool)
    {
        base.Activated(pool);
        EnemyInformationSet.Clear();
        updateCellRange = true;
    }

    public bool IsInCell(Vector3 position)
    {
        bool returnFlag = false;
        Rect rect = CellRange;
        if(updateCellRange == false)
        {
            returnFlag = rect.Contains(position, false);
        }
        return returnFlag;
    }

    public void ExchangeEnemyInformation(List<IEnemy> enemies)
    {
        Vector3 shipPosition = ShipController.Position;
        int index = 0;
        
        // Check if there are any enemies to add
        IEnemyInfo enemyInfo = null;
        for(index = 0; index < EnemyInformationSet.Count; ++index)
        {
            // Check the distance of the enemy from the ship
            enemyInfo = EnemyInformationSet[index];
            if(IGameManager.DistanceSquared(enemyInfo.Position, shipPosition) < Manager.ResourceRangeSqr)
            {
                // If the enemy is close enough to the ship, generate a new enemy
                enemies.Add(enemyInfo.GenerateEnemy());
                
                // Remove the enemy information from the cell
                EnemyInformationSet.RemoveAt(index);
                --index;
            }
        }
        
        // Gather all the enemies we're removing
        IEnemy enemy = null;
        Vector3 enemyPosition;
        for(index = 0; index < enemies.Count; ++index)
        {
            enemy = enemies[index];
            if (enemy.gameObject.activeSelf == false)
            {
                // If the enemy is inactive, remove it from the manager's list
                enemies.RemoveAt(index);
                --index;
            }
            else
            {
                // Check if the enemy is in this cell, but too far away from the ship...
                enemyPosition = enemy.transform.position;
                if((IGameManager.DistanceSquared(enemyPosition, shipPosition) > Manager.ResourceRangeSqr) &&
                   (IsInCell(enemyPosition) == true))
                {
                    // Generate the enemy's information
                    EnemyInformationSet.Add(enemy.GenerateInfo());
                    
                    // Destroy the enemy,
                    enemy.Destroy(null);
                    
                    // Remove the enemy from the manager's list
                    enemies.RemoveAt(index);
                    --index;
                }
            }
        }
    }

    public void UpdateBackground()
    {
        BackgroundManager.Instance.DisplayedBackground = backgroundType;
    }

    #region Protected Stuff

    protected Transform CellTransform
    {
        get
        {
            if(cellTransform == null)
            {
                cellTransform = GetComponent<Transform>();
            }
            return cellTransform;
        }
    }

    protected List<IEnemyInfo> EnemyInformationSet
    {
        get
        {
            return enemyInformationSet;
        }
    }
    
    protected virtual bool UpdateCellRange()
    {
        // Make sure the manager and transform are available
        bool isUpdated = false;
        if((updateCellRange == true) && (Manager != null) && (Manager.Grid != null) && (CellTransform != null))
        {
            // Grab the height and width from the manager
            cellRange.height = Manager.Grid.cellSize;
            cellRange.width = Manager.Grid.cellSize;

            // Grab the center from the transform
            cellRange.center = new Vector2(Position.x, Position.y);

            // Update flags
            updateCellRange = false;
            isUpdated = true;
        }
        return isUpdated;
    }

    #endregion
}
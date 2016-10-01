using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : IGameManager
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down
	}

	[Tooltip("The length, in number of GridCell, of each side of the square grid.")]
	[Range(3, 10)]
	public int gridSize = 3;
	[Tooltip("The length, in meters, of each GridCell size.")]
	[Range(50, 250)]
	public float cellSize = 10;
	[Tooltip("Distance that something has to be from the ship to be activated.")]
	[Range(5, 100)]
	public float resourceRange = 40;
    public ShopCell shopCell = null;
    public IGridCell[] normalCells = null;
    public IGridCell[] specialCells = null;

	readonly List<IDestructable> destructables = new List<IDestructable>();
	readonly List<IEnemy> enemies = new List<IEnemy>();
    readonly List<IGridCell> availableCellCacheList = new List<IGridCell>();

    RogueGrid grid = null;
    bool gridReady = false;
	List<IGridCell> tempCells = new List<IGridCell>(9);
    float resourceRangeSqr = -1f;

	#region Overrides

	public override int NumDestructables
	{
		get
		{
			return (enemies.Count + destructables.Count);
		}
	}
	
	public override IEnumerable<IDestructable> AllDestructables()
	{
        int index = 0;
        for(; index < enemies.Count; ++index)
		{
			yield return enemies[index];
		}
        for(index = 0; index < destructables.Count; ++index)
        {
            yield return destructables[index];
        }
	}

	public override void AddEnemy(IEnemy enemy)
	{
		if(enemy != null)
		{
			enemies.Add(enemy);
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

    public float ResourceRangeSqr
    {
        get
        {
            if(resourceRangeSqr < 0)
            {
                resourceRangeSqr = resourceRange;
                resourceRangeSqr *= IGameManager.ScreenRatioMultiplier;
                resourceRangeSqr *= resourceRangeSqr;
            }
            return resourceRangeSqr;
        }
    }

    public RogueGrid Grid
    {
        get
        {
            return grid;
        }
    }

	// Use this for initialization
	void Start ()
	{
		// Update cell information
        int index = 0;
        SetupCell(shopCell, 0);
        for(index = 0; index < normalCells.Length; ++index)
        {
            SetupCell(normalCells[index], (index + 1));
        }
        for(index = 0; index < specialCells.Length; ++index)
        {
            SetupCell(specialCells[index], (index + normalCells.Length + 1));
        }

		// Generate a new grid from the collected cells
		ShipController.Instance.startEvent += GenerateANewGrid;
		ShipController.Instance.deadEvent += DestroyAllDestructables;
	}
	
	// Update is called once per frame
	void Update()
	{
        if(gridReady == true)
		{
			// Go through all the active cells, and exchange information
			grid.CenterCell.ExchangeEnemyInformation(enemies);
			foreach(IGridCell cell in grid.SurroundingCells)
			{
                cell.ExchangeEnemyInformation(enemies);
			}

			// Go through all destructables
			Vector3 shipPosition = ShipController.Instance.transform.position;
			IDestructable destructable = null;
			for(int index = 0; index < destructables.Count; ++index)
			{
				// Check if this destructable is within maintainable distance
				destructable = destructables[index];
                if(IGameManager.DistanceSquared(destructable.transform.position, shipPosition) > ResourceRangeSqr)
				{
					// If not, destroy the destructable
					destructable.Destroy(null);
					destructables.RemoveAt(index);
					--index;
				}
			}

			// Update the grid
			UpdateGrid(shipPosition);
		}
	}

	#region Helper Methods

    void SetupCell(IGridCell cell, int id)
    {
        cell.TypeIdentifier = normalCells.Length;
        cell.Manager = this;
        cell.OriginalName = cell.name;
    }

    void GenerateANewGrid(ShipController instance)
    {
        // Setup variables
        int index = 0, x = 0, y = 0;

        // Check if the grid is created or not
        if(grid == null)
        {
            // Create a new grid
            grid = new RogueGrid(gridSize, cellSize);
        }
        else
        {
            // Deactivate all previously existing GridCells (makes pooling easier)
            for(index = 0; index < grid.numCells; ++index)
            {
                x = (index % gridSize);
                y = (index / gridSize);
                
                // If a cell existed before, deactivate it
                grid[x, y] = null;
            }
        }
        
        // Fill the grid with new cells
        int gridSizeHalf = (gridSize / 2);
        for(index = 0; index < grid.numCells; ++index)
        {
            x = (index % gridSize);
            y = (index / gridSize);
            
            // Generate a new cell
            GetSurroundingCells(x, y, ref tempCells);
            if((x == gridSizeHalf) && (y == gridSizeHalf))
            {
                // Grab a version of this cell from the pooling manager
                grid[x, y] = GetCellFromPool(shopCell);
            }
            else
            {
                grid[x, y] = GetCell(x, y, tempCells);
            }
        }
        
        // Update which cells are activated
        UpdateActivatedCells(null);

        // Update the background
        grid.CenterCell.UpdateBackground();
        gridReady = true;
    }
    
    IGridCell GetCell(int x, int y, List<IGridCell> excludeCells)
	{
		bool exclude = true;
		List<IGridCell> cells = GetAvailableCells();
		IGridCell returnCell = null;
		int randomIndex = 0;

		// Check if there are any cells available
		if(cells.Count > 0)
		{
			// Check if we want to exclude any cells
			if(excludeCells.Count >= cells.Count)
			{
				exclude = false;
			}

			// Grab a random cell
			randomIndex = Random.Range(0, cells.Count);
			returnCell = cells[randomIndex];

			// Check if we need to exclude a cell
			if(exclude == true)
			{
				// Check if this cell should be excluded
				bool incrementRandomIndex = (Random.value > 0.5f);
				if(Contains(excludeCells, returnCell) == true)
				{
					returnCell = null;
				}

				// Check if this cell was excluded
				while(returnCell == null)
				{
					// Update the index
					if(incrementRandomIndex == true)
					{
						++randomIndex;
						if(randomIndex >= cells.Count)
						{
							randomIndex = 0;
						}
					}
					else
					{
						--randomIndex;
						if(randomIndex < 0)
						{
							randomIndex = (cells.Count - 1);
						}
					}

					// Grab a cell
					returnCell = cells[randomIndex];

					// Check if this cell should be excluded
                    if(Contains(excludeCells, returnCell) == true)
					{
						returnCell = null;
					}
				}
			}

            // Grab a version of this cell from the pooling manager
            returnCell = GetCellFromPool(returnCell);
		}
		return returnCell;
	}

    bool Contains(List<IGridCell> excludeCells, IGridCell returnCell)
    {
        bool returnFlag = false;
        for(int index = 0; index < excludeCells.Count; ++index)
        {
            if(excludeCells[index].TypeIdentifier == returnCell.TypeIdentifier)
            {
                returnFlag = true;
                break;
            }
        }
        return returnFlag;
    }

    IGridCell GetCellFromPool(IGridCell originalCell)
    {
        // Clone this cell
        int identifier = originalCell.TypeIdentifier;
        string originalName = originalCell.OriginalName;
        GameObject clonedCell = GlobalGameObject.Get<PoolingManager>().GetInstance(originalCell.CellObject);
        IGridCell returnCell = clonedCell.GetComponent<IGridCell>();

        // Update the cell's information
        returnCell.Manager = this;
        returnCell.TypeIdentifier = identifier;
        returnCell.OriginalName = originalName;
        return returnCell;
    }

	List<IGridCell> GetAvailableCells()
	{
		availableCellCacheList.Clear();
		for(int index = 0; index < normalCells.Length; ++index)
		{
			if(normalCells[index].IsAvailable == true)
			{
				availableCellCacheList.Add(normalCells[index]);
			}
		}
		return availableCellCacheList;
	}

	void DestroyAllDestructables(ShipController instance)
	{
		// Destroy everything
        int index = 0;
        for(; index < enemies.Count; ++index)
        {
            enemies[index].Destroy(this);
        }
        for(index = 0; index < destructables.Count; ++index)
        {
            destructables[index].Destroy(this);
        }
		enemies.Clear();
		destructables.Clear();

        // Indicate the grid is no longer ready
        gridReady = false;
	}

	void UpdateActivatedCells(Direction? shipDirection)
	{
		// First, deactivate previously active cells based on ship direction
		int index = 0;
        if(shipDirection.HasValue == true)
        {
            tempCells.Clear();
            if((shipDirection.Value == Direction.Up) && (grid.size > 4))
            {
                // Deactive the 3 cells 2 units below the center cell
                for(index = -1; index <= 1; ++index)
                {
                    tempCells.Add(grid[(grid.sizeHalf + index), (grid.sizeHalf + 2)]);
                }
            }
            else if((shipDirection.Value == Direction.Left) && (grid.size > 4))
            {
                // Deactive the 3 cells 2 units right of the center cell
                for(index = -1; index <= 1; ++index)
                {
                    tempCells.Add(grid[(grid.sizeHalf + 2), (grid.sizeHalf + index)]);
                }
            }
            else if((shipDirection.Value == Direction.Down) && (grid.size > 3))
            {
                // Deactive the 3 cells 2 units above the center cell
                for(index = -1; index <= 1; ++index)
                {
                    tempCells.Add(grid[(grid.sizeHalf + index), (grid.sizeHalf - 2)]);
                }
            }
            else if((shipDirection.Value == Direction.Right) && (grid.size > 3))
            {
                // Deactive the 3 cells 2 units left of the center cell
                for(index = -1; index <= 1; ++index)
                {
                    tempCells.Add(grid[(grid.sizeHalf - 2), (grid.sizeHalf + index)]);
                }
            }

            // Deactivate the collected cells
            for(index = 0; index < tempCells.Count; ++index)
    		{
                if(tempCells[index] != null)
    			{
                    tempCells[index].Deactivate();
    			}
    		}
        }

		// Gather all the surrounding cells
		int x = 0, y = 0;
		for(index = 0; index < 9; ++index)
		{
			// Grab the x and y coordinates
			x = ((index % 3) - 1) + grid.sizeHalf;
            y = ((index / 3) - 1) + grid.sizeHalf;

			// Check the cell's state
            if(grid[x, y].CurrentState != IGridCell.State.Active)
            {
    			if(grid[x, y].CurrentState == IGridCell.State.InactiveExpired)
    			{
    				// If the cell expired, replace it with a new one
    				GetSurroundingCells(x, y, ref tempCells);
                    grid[x, y] = null;
                    grid[x, y] = GetCell(x, y, tempCells);
    			}
    			else if((grid[x, y].CurrentState == IGridCell.State.InactiveNew) && (grid[x, y].IsAvailable == false))
    			{
    				// If the cell is new, but no longer available, replace it with a new one
    				GetSurroundingCells(x, y, ref tempCells);
                    grid[x, y] = null;
                    grid[x, y] = GetCell(x, y, tempCells);
    			}

    			// Activate this cell
    			grid[x, y].Activate();
            }
		}
	}

	void GetSurroundingCells(int centerX, int centerY, ref List<IGridCell> excludeList)
	{
		// Make sure we always have a valid list
		if(excludeList == null)
		{
			excludeList = new List<IGridCell>(9);
		}
		else
		{
			excludeList.Clear();
		}

		// Find all surrounding cells
		int index = 0, x = 0, y = 0;
		for(index = 0; index < 9; ++index)
		{
			// Grab the x and y coordinates
			x = ((index % 3) - 1) + centerX;
			y = ((index / 3) - 1) + centerY;

			// Make sure these coordinates are valid
            if((x >= 0) && (y >= 0) && (x < grid.size) && (y < grid.size))
			{
				// Add the grid cell into the list
				if(grid[x, y] != null)
				{
					excludeList.Add(grid[x, y]);
				}
			}
		}
	}

	void UpdateGrid(Vector3 shipPosition)
	{
		// Check if the ship escaped the current cell
        if((grid.CenterCell.IsInCell(shipPosition) == false) && (grid.CenterCell.IsReady == true))
		{
			// Determine the direction the ship is heading.  Prioritize on the horizontals.
			Direction shipDirection = Direction.Up;
			if(shipPosition.x < grid.CenterCell.CellRange.xMin)
			{
				shipDirection = Direction.Left;
			}
			else if(shipPosition.x > grid.CenterCell.CellRange.xMax)
			{
				shipDirection = Direction.Right;
			}
			else if(shipPosition.y < grid.CenterCell.CellRange.yMin)
			{
				shipDirection = Direction.Down;
			}

            //Debug.Log("Ship moved to " + shipDirection);

            // Shift the grid in the opposite direction of the ship
            grid.ShiftGridInOppositeDirection(shipDirection);

			// Create new column/row of cells that are "empty" after the shift
			PopulateEmptyCells(shipDirection);

			// Update which cells are now going to be active
			UpdateActivatedCells(shipDirection);

            // Update the background based on the center cell's settings
            grid.CenterCell.UpdateBackground();
		}
	}

	void PopulateEmptyCells(Direction shipDirection)
	{
		// Create new column/row of cells that are "empty" after the shift
        int x = 0, y = 0;

		if(shipDirection == Direction.Right)
		{
			// The right column is empty
			x = gridSize - 1;
		}
		else if(shipDirection == Direction.Down)
		{
			// The bottom row is empty
			y = gridSize - 1;
		}

		// Get direction
		switch(shipDirection)
		{
    		case Direction.Left:
    		case Direction.Right:
    			// Go though a column of cells, and generate new cells
    			for (y = 0; y < gridSize; ++y)
    			{
    				// Generate a new cell
                    //Debug.Log("New cell: (" + x + " x " + y + ')');
    				GetSurroundingCells(x, y, ref tempCells);
                    grid[x, y] = GetCell(x, y, tempCells);
       			}
    			break;
    		default:
    			// Go though a row of cells, and generate new cells
    			for(x = 0; x < gridSize; ++x)
    			{
    				// Generate a new cell
                    //Debug.Log("New cell: (" + x + " x " + y + ')');
    				GetSurroundingCells(x, y, ref tempCells);
                    grid[x, y] = GetCell(x, y, tempCells);
    			}
    			break;
		}
	}

	#endregion
}

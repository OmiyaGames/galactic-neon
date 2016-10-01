using UnityEngine;
using System.Collections.Generic;

public class RogueGrid
{
    public const int NumSurroundingCells = 8;

    public readonly int size;
    public readonly int sizeHalf;
    public readonly int numCells;
    public readonly float cellSize;

    readonly IGridCell[,] grid;
    readonly int[,] surroundingCellCoordinates = new int[NumSurroundingCells, 2];

    public RogueGrid(int newGridSize, float newCellSize)
    {
        if(newGridSize < 3)
        {
            throw new System.ArgumentOutOfRangeException("newGridSize", "newGridSize has to be 3 or greater");
        }
        else if(newCellSize < 10)
        {
            throw new System.ArgumentOutOfRangeException("cellSize", "newCellSize has to be 10 or greater");
        }

        // Update member variables
        cellSize = newCellSize;
        size = newGridSize;
        sizeHalf = newGridSize / 2;
        numCells = newGridSize * newGridSize;

        // Set the grid to the grid size
        grid = new IGridCell[size, size];

        // Setup the surrounding cell coordinates
        int index = 0, innerIndex = 0;
        for(; index < NumSurroundingCells; ++index)
        {
            // Get x and y coordinates
            innerIndex = index;
            if(innerIndex > (NumSurroundingCells / 2))
            {
                ++innerIndex;
            }

            // Set the coordinates
            surroundingCellCoordinates[index, 0] = ((innerIndex % 3) - 1) + sizeHalf;
            surroundingCellCoordinates[index, 1] = ((innerIndex / 3) - 1) + sizeHalf;
        }
    }

    #region Properties

    public IGridCell this[int x, int y]
    {
        get
        {
            return grid[x, y];
        }
        set
        {
            // Check if we need to update the cell
            if(value != null)
            {
                SetupCell(ref value, x, y, CenterPosition);
            }
            else if(grid[x, y] != null)
            {
                // Rename the cell
                grid[x, y].name = grid[x, y].OriginalName + " (Deactivated)";

                // Deactivate the previous grid cell
                grid[x, y].Deactivate();
                grid[x, y].CellObject.SetActive(false);
            }

            // Update Grid
            grid[x, y] = value;
        }
    }

    public IGridCell CenterCell
    {
        get
        {
            return grid[sizeHalf, sizeHalf];
        }
    }

    public IEnumerable<IGridCell> SurroundingCells
    {
        get
        {
            for(int index = 0; index < NumSurroundingCells; ++index)
            {
                yield return grid[surroundingCellCoordinates[index, 0], surroundingCellCoordinates[index, 1]];
            }
        }
    }

    public Vector3 CenterPosition
    {
        get
        {
            if(CenterCell == null)
            {
                return ShipController.Position;
            }
            else
            {
                return CenterCell.Position;
            }
        }
    }

    #endregion

    public void ShiftGridInOppositeDirection(GridManager.Direction shipDirection)
    {
        // Deactivate column/row of cells that are going to "leak" after the shift
        DeactivateLeakingCells(shipDirection);
        
        // Shift the grid in the opposite direction of the ship
        ShiftCells(shipDirection);

        // Mark the left-over column/row as null
        EmptyCells(shipDirection);
    }

    void DeactivateLeakingCells(GridManager.Direction shipDirection)
    {
        // Deactivate column/row of cells that are going to "leak" after the shift
        int x = 0, y = 0;
        if(shipDirection == GridManager.Direction.Left)
        {
            // The right column will leak
            x = size - 1;
        }
        else if(shipDirection == GridManager.Direction.Up)
        {
            // The bottom row will leak
            y = size - 1;
        }
        switch(shipDirection)
        {
            case GridManager.Direction.Left:
            case GridManager.Direction.Right:
                // Go though a column of cells
                for(y = 0; y < size; ++y)
                {
                    // Deactivate the cells
                    //Debug.Log("Leaking cell: (" + x + " x " + y + ')');
                    this[x, y] = null;
                }
                break;
            default:
                // Go though a row of cells
                for(x = 0; x < size; ++x)
                {
                    // Deactivate the cells
                    //Debug.Log("Leaking cell: (" + x + " x " + y + ')');
                    this[x, y] = null;
                }
                break;
        }
    }
    
    void ShiftCells(GridManager.Direction shipDirection)
    {
        // Shift the grid in the opposite direction of the ship
        int x = 0, y = 0;
        Vector3 newCenterPosition;
        switch(shipDirection)
        {
            case GridManager.Direction.Left:
                // Shift all cells to the right
                newCenterPosition = grid[(sizeHalf - 1), sizeHalf].Position;
                //Debug.Log("Moving center from " + CenterPosition.ToString() + " to " + newCenterPosition.ToString());
                for(x = (size - 1); x > 0; --x)
                {
                    for (y = 0; y < size; ++y)
                    {
                        //Debug.Log("Shift cell: (" + (x - 1) + " x " + y + ") to (" + x + " x " + y + ')');
                        grid[x, y] = grid[(x - 1), y];

                        // Setup the cell to the new position
                        SetupCell(ref grid[x, y], x, y, newCenterPosition);
                    }
                }
                break;
            case GridManager.Direction.Right:
                // Shift all cells to the left
                newCenterPosition = grid[(sizeHalf + 1), sizeHalf].Position;
                //Debug.Log("Moving center from " + CenterPosition.ToString() + " to " + newCenterPosition.ToString());
                for(x = 0; x < (size - 1); ++x)
                {
                    for (y = 0; y < size; ++y)
                    {
                        //Debug.Log("Shift cell: (" + (x + 1) + " x " + y + ") to (" + x + " x " + y + ')');
                        grid[x, y] = grid[(x + 1), y];

                        // Setup the cell to the new position
                        SetupCell(ref grid[x, y], x, y, newCenterPosition);
                    }
                }
                break;
            case GridManager.Direction.Up:
                // Shift all cells to the down
                newCenterPosition = grid[sizeHalf, (sizeHalf - 1)].Position;
                //Debug.Log("Moving center from " + CenterPosition.ToString() + " to " + newCenterPosition.ToString());
                for(x = 0; x < size; ++x)
                {
                    for (y = (size - 1); y > 0; --y)
                    {
                        //Debug.Log("Shift cell: (" + x + " x " + (y - 1) + ") to (" + x + " x " + y + ')');
                        grid[x, y] = grid[x, (y - 1)];

                        // Setup the cell to the new position
                        SetupCell(ref grid[x, y], x, y, newCenterPosition);
                    }
                }
                break;
            default:
                // Shift all cells to the up
                newCenterPosition = grid[sizeHalf, (sizeHalf + 1)].Position;
                //Debug.Log("Moving center from " + CenterPosition.ToString() + " to " + newCenterPosition.ToString());
                for(x = 0; x < size; ++x)
                {
                    for (y = 0; y < (size - 1); ++y)
                    {
                        //Debug.Log("Shift cell: (" + x + " x " + (y + 1) + ") to (" + x + " x " + y + ')');
                        grid[x, y] = grid[x, (y + 1)];

                        // Setup the cell to the new position
                        SetupCell(ref grid[x, y], x, y, newCenterPosition);
                    }
                }
                break;
        }
    }

    void EmptyCells(GridManager.Direction shipDirection)
    {
        // Create new column/row of cells that are "empty" after the shift
        int x = 0, y = 0;
        
        if(shipDirection == GridManager.Direction.Right)
        {
            // The right column is empty
            x = size - 1;
        }
        else if(shipDirection == GridManager.Direction.Down)
        {
            // The bottom row is empty
            y = size - 1;
        }
        
        // Get direction
        switch(shipDirection)
        {
            case GridManager.Direction.Left:
            case GridManager.Direction.Right:
                // Go though a column of cells, and empty cell
                for (y = 0; y < size; ++y)
                {
                    //Debug.Log("Empty cell: (" + x + " x " + y + ')');

                    // Only set the cell to null; do not deactivate it
                    grid[x, y] = null;
                }
                break;
            default:
                // Go though a row of cells, and empty cell
                for(x = 0; x < size; ++x)
                {
                    // Generate a new cell
                    //Debug.Log("Empty cell: (" + x + " x " + y + ')');

                    // Only set the cell to null; do not deactivate it
                    grid[x, y] = null;
                }
                break;
        }
    }

    void SetupCell(ref IGridCell cell, int x, int y, Vector3 center)
    {
        // Rename the cell
        cell.name = cell.OriginalName + " (" + x + " x " + y + ')';

        // Update position
        cell.Position = GetNewCellPosition(x, y, center);

        // Update cell coordinates
        cell.X = x;
        cell.Y = y;
    }

    Vector3 GetNewCellPosition(int x, int y, Vector3 center)
    {
        Vector3 cellPosition = center;
        cellPosition.x += (x - sizeHalf) * cellSize;
        cellPosition.y += (sizeHalf - y) * cellSize;
        return cellPosition;
    }
}

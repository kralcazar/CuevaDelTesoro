using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int cols, rows;
    [SerializeField]
    private float cellSize;

    private Grid<Cell> grid;

    public static GridManager instance;
    private void Start()
    {
        if (instance == null) instance = this;
    }
    // Start is called before the first frame update
    public void GenerateGrid(int gridSize)
    {
        rows = gridSize;
        cols = gridSize;

        if (instance == null)
        {
            instance = this;
        }

        grid = new Grid<Cell>(cols, rows, cellSize, (Grid<Cell> g, int x, int y) => new Cell(g, x, y), transform);

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Cell cell = grid.GetGridObject(x, y);
                cell.spriteR.sprite = Resources.Load<Sprite>("cellEmpty");
            }
        }

        //Centering the grid
        float gridW = cols * cellSize;
        float gridH = rows * cellSize;
        transform.position = new Vector2(-gridW / 2 , -gridH / 2 );
    }

    public static void DropElement(CellType cellType)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x, y;
        instance.grid.GetXY(worldPosition, out x, out y);
        if (instance.grid.XYInGrid(x, y))
        {
            Debug.Log(cellType + " to " + x + " : " + y);
            instance.grid.GetGridObject(worldPosition).SetCellType(cellType);
        }
    }

    public static Grid<Cell> GetGrid()
    {
        return instance.grid;
    }
}

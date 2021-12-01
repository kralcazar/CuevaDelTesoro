using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType { Empty, Player, Tresor, Cliff, Monster}
public class Cell 
{

    private Grid<Cell> grid;
    private int x;
    private int y;
    private CellType cellType;
    public GameObject cellGO;
    public SpriteRenderer spriteR;

    public Cell(Grid<Cell> grid, int x, int y)
    {
        Debug.Log("Cell: "+x+","+y);
        this.grid = grid;
        this.x = x;
        this.y = y;
        cellType = CellType.Empty;
        cellGO = new GameObject(x+":"+y);
        cellGO.transform.parent = grid.GetGridTransform();
        cellGO.transform.position = grid.GetWorldPosition(x, y) + Vector3.right * grid.GetCellSize() / 2 + Vector3.up * grid.GetCellSize() / 2;
        spriteR = cellGO.AddComponent<SpriteRenderer>();        
    }

    public void SetCellType(CellType cellType)
    {
        this.cellType = cellType;
        grid.TriggerGridObjectChanged(x, y);
        switch (cellType)
        {
            case CellType.Empty:
                spriteR.sprite = Resources.Load<Sprite>("cellEmpty");
                break;
            case CellType.Player:
                spriteR.sprite = Resources.Load<Sprite>("cellPlayer");
                break;
            case CellType.Tresor:
                spriteR.sprite = Resources.Load<Sprite>("cellTresor");
                break;
            case CellType.Cliff:
                spriteR.sprite = Resources.Load<Sprite>("cellCliff");
                break;
            case CellType.Monster:
                spriteR.sprite = Resources.Load<Sprite>("cellMonster");
                break;
        }
    }

    public CellType GetCellType()
    {
        return cellType;
    }

    public override string ToString()
    {
        return cellType.ToString();
    }
}

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int cols, rows;
    [SerializeField]
    private float cellSize;

    private Grid<Cell> grid;

    private GameObject cellEmpty, cellPlayer, cellTresor, cellCliff, cellMonster;

    // Start is called before the first frame update
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x, y;
            grid.GetXY(worldPosition, out x, out y);
            Debug.Log(x + " : " + y);
            Debug.Log(grid.GetGridObject(worldPosition).GetCellType().ToString());
            grid.GetGridObject(worldPosition).SetCellType(CellType.Player);
        }
    }
}

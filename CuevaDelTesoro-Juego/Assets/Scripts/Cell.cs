using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CellType { Empty, Player, Tresor, Cliff, Monster }
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
        this.grid = grid;
        this.x = x;
        this.y = y;
        cellType = CellType.Empty;
        cellGO = new GameObject(x + ":" + y);
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnowledgeBase
{
    //<Cell, <Epoch, Knowledge>>
    private Dictionary<Vector2, Dictionary<int, bool[]>> knowledge = new Dictionary<Vector2, Dictionary<int, bool[]>>();

    //<Cell, Visited>
    private Dictionary<Vector2, bool> knowledgeVisited = new Dictionary<Vector2, bool>();

    public void Inform(Vector2 gridPosition, bool[] knowledgeVector, int epoch)
    {
        Dictionary<int, bool[]> cellPerception;
        if (!knowledge.ContainsKey(gridPosition))
        {
            cellPerception = new Dictionary<int, bool[]>();
            cellPerception.Add(epoch, knowledgeVector);
            knowledge.Add(gridPosition, cellPerception);
        }
        else
        {
            knowledge[gridPosition].Add(epoch, knowledgeVector);
        }
    }

    public void InformAction(Vector2 gridPosition)
    {
        knowledgeVisited.Add(gridPosition, true);
    }

    //Si la posición preguntada es segura
    public bool Ask(Vector2 gridPosition)
    {
        //Si no tiene el tesoro (iterar por celdas seguras)

        //Si tiene el tesoro (volver por las celdas ok)

        return false;
    }
}

public class Enviroment
{
    public static bool[] GetPerception(Vector2 gridPosition)
    {
        //Hedor, Brisa, Resplandor, Golpe
        bool[] perception = { false, false, false, false };
        int[] lookX = { -1, 0, 0, 1 };
        int[] lookY = { 0, -1, 1, 0 };
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = (int)gridPosition.x + lookX[i];
            int _y = (int)gridPosition.y + lookY[i];
            if(!GridManager.GetGrid().XYInGrid(_x, _y)) //La casilla está fuera del área (hay un muro).
            {
                perception[3] = true; //Golpe
                continue;
            }
            CellType cellType = GridManager.GetGrid().GetGridObject(_x, _y).GetCellType();
            switch (cellType)
            {
                case CellType.Tresor:
                    perception[2] = true; //Resplandor
                    break;
                case CellType.Cliff:
                    perception[1] = true; //Brisa
                    break;
                case CellType.Monster:
                    perception[0] = true; //Hedor
                    break;
            }
        }
        return perception;
    }
}

public enum Directions { West, North, East, South }

public class Action
{
    public Directions direction;
}

public class Agent : MonoBehaviour
{
    private KnowledgeBase kb;
    private int tickCounter;

    // Start is called before the first frame update
    void Start()
    {
        kb = new KnowledgeBase();
        PercieveAndInformEnvironmentSurroundings(transform.position, tickCounter);
    }

    // Update is called once per frame
    private void Tick()
    {
        PercieveAndInformEnvironmentSurroundings(transform.position, tickCounter);
        Action action;
        if ((action = AskForActions(transform.position)) != null)
        {
            int x, y;
            GridManager.GetGrid().GetXY(transform.position, out x, out y);
            kb.InformAction(new Vector2(x, y));
            tickCounter++;
        }
    }

    //Recibir percepciones del entorno e informar a la base de conocimientos
    private void PercieveAndInformEnvironmentSurroundings(Vector3 worldPosition, int tickCounter)
    {
        int[] lookX = { -1, 0, 0, 1 };
        int[] lookY = { 0, -1, 1, 0 };
        int x, y;
        GridManager.GetGrid().GetXY(worldPosition, out x, out y);
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            kb.Inform(new Vector2(_x, _y), Enviroment.GetPerception(new Vector2(_x, _y)), tickCounter);
        }
    }

    private Action AskForActions(Vector3 worldPosition)
    {
        int x, y;
        GridManager.GetGrid().GetXY(worldPosition, out x, out y);

        int[] lookX = { -1, 0, 1, 0 };
        int[] lookY = { 0, -1, 0, 1 };
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            if(kb.Ask(new Vector2(_x, _y)))
            {
                Action action = new Action();
                switch (i)
                {
                    case 0:
                        action.direction = Directions.West;
                        break;
                    case 1:
                        action.direction = Directions.North;
                        break;
                    case 2:
                        action.direction = Directions.East;
                        break;
                    case 3:
                        action.direction = Directions.South;
                        break;
                }
                transform.position = GridManager.GetGrid().GetWorldPosition(_x, _y);
                return action;
            }
        }
        return null;
    }
}

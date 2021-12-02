using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnowledgeBase
{
    //<Epoch, <Cell, Knowledge>>
    private Dictionary<int, Dictionary<Vector2, bool[]>> knowledge = new Dictionary<int, Dictionary<Vector2, bool[]>>();

    public void Inform(int epoch, Vector2 gridPosition, bool[] knowledgeVector)
    {
        Dictionary<Vector2, bool[]> cellPerception;
        if (!knowledge.ContainsKey(epoch))
        {
            cellPerception = new Dictionary<Vector2, bool[]>();
            cellPerception.Add(gridPosition, knowledgeVector);
            knowledge.Add(epoch, cellPerception);
        }
        else
        {
            knowledge[epoch].Add(gridPosition, knowledgeVector);
        }
    }

    public void Ask()
    {

    }

    public void PerformAction()
    {

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

public class Agent : MonoBehaviour
{
    private KnowledgeBase kb;
    private int tickCounter;
    private bool found;

    // Start is called before the first frame update
    void Start()
    {
        kb = new KnowledgeBase();
        PercieveAndInformSurroundings(transform.position);
    }

    // Update is called once per frame
    private void Tick()
    {
        //poseer determinado conocimiento del entorno
        //kb.PerformAction(perceptions, ); //Tomar acciones en base a las percepciones y conocimiento
        PercieveAndInformSurroundings(transform.position);
    }

    //Recibir percepciones del entorno e informar a la base de conocimientos
    private void PercieveAndInformSurroundings(Vector3 worldPosition)
    {
        int[] lookX = { -1, 0, 0, 1 };
        int[] lookY = { 0, -1, 1, 0 };
        int x, y;
        GridManager.GetGrid().GetXY(worldPosition, out x, out y);
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            kb.Inform(tickCounter, new Vector2(_x, _y), Enviroment.GetPerception(new Vector2(_x, _y)));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnowledgeBase
{
    // Zona de hechos para inferir conocimiento
    //<Cell, Knowledge (Hedor, Brisa, Resplandor, Golpe)>
    private Dictionary<Vector2, bool[]> knowledgePerceptions = new Dictionary<Vector2, bool[]>();

    private Dictionary<Vector2, bool> posibleMonster;
    private Dictionary<Vector2, bool> posibleCliff;
    private Dictionary<Vector2, bool> noMonster;
    private Dictionary<Vector2, bool> noCliff;

    // Conocimiento inferido, se conoce con seguridad el tipo de celda 
    private Dictionary<Vector2, CellType> knowledgeInfered = new Dictionary<Vector2, CellType>();

    //<Cell, Visited>, las celdas visitadas por el agente
    private Dictionary<Vector2, bool> knowledgeVisited = new Dictionary<Vector2, bool>();

    //Meta
    private bool hasTresor;

    public void Inform(Vector2 gridPosition, bool[] knowledgeVector)
    {
        if (!knowledgePerceptions.ContainsKey(gridPosition))
        {
            knowledgePerceptions.Add(gridPosition, knowledgeVector);
        }
        else
        {
            knowledgePerceptions[gridPosition] = knowledgeVector;
        }
    }

    public void InformAction(Vector2 gridPosition)
    {
        knowledgeVisited.Add(gridPosition, true);
    }

    //Aplicar las reglas para inferir conocimiento (deductivo)
    public void InferCell(Vector2 gridPosition)
    {
        int[] lookX = { -1, 0, 1, 0 };
        int[] lookY = { 0, -1, 0, 1 };

        //Si hay resplandor hay un tesoro
        if (knowledgePerceptions[gridPosition][2])
        {
            SetKnowledgeInfered(gridPosition, CellType.Tresor);
        }
        //Si no hay ningún efecto las casillas adyacentes son seguras.
        if(!knowledgePerceptions[gridPosition][0] && !knowledgePerceptions[gridPosition][1] && !knowledgePerceptions[gridPosition][3])
        {
            for (int i = 0; i < lookX.Length; i++)
            {
                Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                SetKnowledgeInfered(adjacentPosition, CellType.Empty);
            }
        }
        else if (knowledgePerceptions[gridPosition][0]) //Hedor
        {
            for (int i = 0; i < lookX.Length; i++) //Posible monstruo en las adyacentes
            {
                Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                if (!posibleMonster.ContainsKey(adjacentPosition))
                {
                    posibleMonster.Add(adjacentPosition, true);
                }
            }
        }
        else if (knowledgePerceptions[gridPosition][1]) //Brisa
        {
            for (int i = 0; i < lookX.Length; i++) //Posible acantilado en las adyacentes
            {
                Vector2 adjacentPosition = new Vector2((int)gridPosition.x + lookX[i], (int)gridPosition.y + lookY[i]);
                if (!posibleCliff.ContainsKey(adjacentPosition))
                {
                    posibleCliff.Add(adjacentPosition, true);
                }
            }
        }

        //Llegar a conclusiones de noCliff y noMonster
        //...

        // Si se sabe que no hay monstruo ni acantilado ni tesoro la celda está vacía
        if(noCliff[gridPosition] && noMonster[gridPosition] && knowledgeInfered[gridPosition] != CellType.Tresor)
        {
            SetKnowledgeInfered(gridPosition, CellType.Empty);
        }
    }

    private void SetKnowledgeInfered(Vector2 gridPosition, CellType cellType)
    {
        if (!knowledgeInfered.ContainsKey(gridPosition))
        {
            knowledgeInfered.Add(gridPosition, cellType);
        }
    }

    //Si la posición preguntada es segura (reactivo)
    public bool Ask(Vector2 gridPosition)
    {
        //Si no tiene el tesoro (iterar por celdas seguras)

        //Si tiene el tesoro (volver por las celdas ok)

        return false;
    }

    //Devuelve la prioridad de la acción a tomar -1: No, 1: Poca prioridad, 2: Mucha prioridad (deductivo)
    public int AskPriority(Vector2 gridPosition)
    {
        if (!hasTresor) //Si no tiene el tesoro (iterar por celdas seguras)
        {
            if (knowledgeInfered.ContainsKey(gridPosition)) // Hay conocimiento sobre el estado de la celda
            {
                if (knowledgeInfered[gridPosition] == CellType.Tresor) return 2;
                else if (knowledgeInfered[gridPosition] == CellType.Empty) return 1;
                else return -1;
            }
        }
        else //Si tiene el tesoro (volver por las celdas ok)
        {

        }
        

        return -1; //No conviene tomar esta casilla
    }
}

public class Enviroment
{
    public static bool[] GetPerception(Vector2 gridPosition)
    {
        //Hedor, Brisa, Resplandor, Golpe
        bool[] perception = { false, false, false, false };
        int x = (int)gridPosition.y;
        int y = (int)gridPosition.y;

        //Si hay un muro es recibe un golpe. Marcaremos las casillas fuera del área con la percepción Golpe.
        if (!GridManager.GetGrid().XYInGrid(x, y)) //La casilla está fuera del área (hay un muro).
        {
            perception[3] = true; //Golpe
            return perception;
        }

        //Ver si la casilla a consultar hay un tesoro
        CellType currentCellType = GridManager.GetGrid().GetGridObject(x, y).GetCellType();
        if (currentCellType == CellType.Tresor)
        {
            perception[2] = true; //Resplandor
        }

        //Mirar el conocimiento del entorno para indicar si hay efectos causados por casillas adyacentes
        int[] lookX = { -1, 0, 1, 0 };
        int[] lookY = { 0, -1, 0, 1 };
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = (int)gridPosition.x + lookX[i];
            int _y = (int)gridPosition.y + lookY[i];
            
            CellType surroundingCellType = GridManager.GetGrid().GetGridObject(_x, _y).GetCellType();
            switch (surroundingCellType)
            {
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

    int[] lookX = { -1, 0, 1, 0 };
    int[] lookY = { 0, -1, 0, 1 };


    // Tick is called once per game frame
    private void Tick()
    {
        int x, y;
        GridManager.GetGrid().GetXY(transform.position, out x, out y);

        PercieveAndInformEnvironmentSurroundings(x, y); // Paso 1: Percibir ambiente e informar a la base de conocimientos

        TryToInfer(x, y); // Paso 2: Inferir conocimiento en la base de conocimientos

        if (AskForActionsDeductive(x, y) != null) // Paso 3: Toma de decisiones
        {
            kb.InformAction(new Vector2(x, y)); // Paso 4: Informar de la acción tomada (útil para marcar la celda ya visitada)
        }
        else
        {
            Debug.LogWarning("No actions allowed");
        }
    }

    //Recibir percepciones del entorno e informar a la base de conocimientos
    private void PercieveAndInformEnvironmentSurroundings(int x, int y)
    {
        for (int i = 0; i < lookX.Length; i++) // Mirar alrededor
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            kb.Inform(new Vector2(_x, _y), Enviroment.GetPerception(new Vector2(_x, _y)));
        }
    }

    private void TryToInfer(int x, int y)
    {
        for (int i = 0; i < lookX.Length; i++) // Inferir alrededor
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            kb.InferCell(new Vector2(_x, _y));
        }
    }

    //Devuelve la primera acción aplicable (agente reactivo)
    private Action AskForActionsReactive(int x, int y)
    {
        Directions[] directions = { Directions.West, Directions.North, Directions.East, Directions.South };
        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            if (kb.Ask(new Vector2(_x, _y)))
            {
                Action action = new Action();
                action.direction = directions[i];
                transform.position = GridManager.GetGrid().GetWorldPosition(_x, _y);
                return action; 
            }
        }
        return null;
    }

    //Devuelve la acción con mayor prioridad (agente deductivo)
    private Action AskForActionsDeductive(int x, int y)
    {
        Directions[] directions = { Directions.West , Directions.North, Directions.East, Directions.South};
        Action[] actions = new Action[lookX.Length];
        int bestPriority = -1;
        int bestActionIndex = -1;

        for (int i = 0; i < lookX.Length; i++)
        {
            int _x = x + lookX[i];
            int _y = y + lookY[i];
            int priority = kb.AskPriority(new Vector2(_x, _y));

            if (priority > bestPriority)
            {
                bestPriority = priority;
                bestActionIndex = i;
            }

            actions[i] = new Action();
            actions[i].direction = directions[i];
        }

        transform.position = GridManager.GetGrid().GetWorldPosition(x + lookX[bestActionIndex], y + lookY[bestActionIndex]);

        if (bestActionIndex != -1)
        {
            return actions[bestActionIndex]; 
        }
        else
        {
            return null;
        }
    }

}
